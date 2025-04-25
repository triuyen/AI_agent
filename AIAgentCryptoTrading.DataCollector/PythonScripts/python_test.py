# copy from collab
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from sklearn.ensemble import RandomForestRegressor
from sklearn.model_selection import (train_test_split, GridSearchCV,
                                   PredefinedSplit, TimeSeriesSplit, KFold, RandomizedSearchCV)
from sklearn.preprocessing import OneHotEncoder, StandardScaler # Import StandardScaler
from sklearn.compose import ColumnTransformer
from sklearn.metrics import (mean_absolute_error, mean_squared_error,
                            r2_score, mean_absolute_percentage_error)
from sklearn.inspection import permutation_importance
from scipy.stats import spearmanr
import pickle

# --- Configuration ---
file_path = 'comprehensive_daily_crypto_data_with_derived_ohlc.csv' # Your file name
price_column = 'price' # The column containing the price you want to predict
date_column = 'timestamp' # The column containing the date/timestamp
grouping_column = 'crypto_id' # Column to identify cryptocurrencies for grouping
# List of all 10 cryptocurrencies
crypto_list = ["bitcoin", "ethereum", "binancecoin", "cardano", "dogecoin",
               "ripple", "solana", "tether", "tron", "usd-coin"]

# Column to drop due to many nulls
column_to_drop = 'max_supply'

# Window size for the Simple Moving Average baseline
sma_window = 5 # You can experiment with different window sizes

# --- Evaluation metrics function ---
def calculate_metrics(y_true, y_pred):
    y_true = np.asarray(y_true).flatten()
    y_pred = np.asarray(y_pred).flatten()

    valid_indices = y_true != 0

    if np.any(valid_indices):
         mape = np.mean(np.abs((y_true[valid_indices] - y_pred[valid_indices]) / y_true[valid_indices])) * 100
    else:
         mape = np.nan

    r2 = r2_score(y_true, y_pred)

    try:
        spearman, _ = spearmanr(y_true, y_pred)
    except ValueError:
        spearman = np.nan

    error_std = np.std(y_true - y_pred)


    return {
        'MAE': mean_absolute_error(y_true, y_pred),
        'MSE': mean_squared_error(y_true, y_pred),
        'RMSE': np.sqrt(mean_squared_error(y_true, y_pred)),
        'R²': r2,
        'MAPE': mape,
        'Spearman': spearman,
        'Error Std': error_std
    }


# --- Load and Prepare Data ---

try:
    df = pd.read_csv(file_path)
    print(f"Shape after loading CSV: {df.shape}")

    # Filter for the specified cryptocurrencies
    df = df[df[grouping_column].isin(crypto_list)].copy()
    print(f"Shape after filtering for crypto_list: {df.shape}")

    # Convert timestamp to datetime and sort by crypto and then date
    # This sorting is needed for correct rolling calculations PER cryptocurrency
    df[date_column] = pd.to_datetime(df[date_column])
    df = df.sort_values(by=[grouping_column, date_column]).reset_index(drop=True)
    print(f"Shape after sorting by crypto and date: {df.shape}")


    # Convert 'last_updated' to numerical features (Unix timestamp)
    initial_rows = df.shape[0]
    df['last_updated'] = pd.to_datetime(df['last_updated'], errors='coerce')
    df.dropna(subset=['last_updated'], inplace=True) # Drop rows where last_updated is NaT
    df['last_updated'] = df['last_updated'].astype(np.int64) // 10**9
    print(f"Shape after processing 'last_updated' (dropped {initial_rows - df.shape[0]} rows): {df.shape}")


    # Ensure the price column is numeric and drop NaNs
    initial_rows = df.shape[0]
    df[price_column] = pd.to_numeric(df[price_column], errors='coerce')
    df.dropna(subset=[price_column], inplace=True) # Drop rows with missing prices
    print(f"Shape after processing '{price_column}' (dropped {initial_rows - df.shape[0]} rows): {df.shape}")


    # Drop the specified column with many nulls
    if column_to_drop in df.columns:
        initial_rows = df.shape[0]
        df.drop(columns=[column_to_drop], inplace=True)
        print(f"Shape after dropping '{column_to_drop}' (dropped 0 rows at this step): {df.shape}")


    # Handle potential infinite values in numerical columns after any calculations
    # Note: This dropna will happen *after* SMA calculation
    # initial_rows = df.shape[0] # Moved initial_rows capture


except FileNotFoundError:
    print(f"Error: File not found at {file_path}")
    exit()
except Exception as e:
    print(f"Error loading or processing data: {e}")
    exit()


# --- Calculate Simple Moving Average Baseline Predictions ---
# Calculate SMA grouped by cryptocurrency BEFORE the final dropna
print(f"\n--- Calculating Simple Moving Average ({sma_window}-day window) Baseline ---")

# Calculate the SMA for each cryptocurrency
df['SMA'] = df.groupby(grouping_column)[price_column].rolling(window=sma_window).mean().reset_index(level=0, drop=True)

# The prediction for time t+1 is the SMA at time t
# Shift by 1 within each cryptocurrency group
df['SMA_Prediction'] = df.groupby(grouping_column)['SMA'].shift(1)

# Now perform the final dropna after SMA calculation to retain data for rolling average
initial_rows = df.shape[0]
df.dropna(inplace=True) # Drop rows with any remaining NaNs (including those from SMA calculation)
print(f"Shape after calculating SMA and final NaNs removal (dropped {initial_rows - df.shape[0]} rows): {df.shape}")


# --- Temporal split (75% train, 25% test) ---
# Split the data based on the original chronological order (overall timestamp)
# Need to re-sort by timestamp ONLY for the temporal split across all coins
df_sorted_by_date = df.sort_values(by=[date_column]).copy()

train_size = int(0.75 * len(df_sorted_by_date))
train_df = df_sorted_by_date.iloc[:train_size].copy() # Keep for reference if needed
test_df = df_sorted_by_date.iloc[train_size:].copy()

print(f"\nTesting data shape for Baselines: {test_df.shape}")


# --- Evaluate the SMA Baseline (Overall and per Cryptocurrency) ---

# Filter SMA predictions to match the test set
sma_test_predictions_df = test_df.copy()
# Ensure actual prices are aligned with the predictions (already aligned by test_df structure)
actual_test_prices_sma = sma_test_predictions_df[price_column]
sma_predictions = sma_test_predictions_df['SMA_Prediction']


print("\n--- Simple Moving Average Baseline Metrics ---")

# Calculate OVERALL SMA metrics for the entire test set
if len(sma_predictions) > 0:
    overall_sma_metrics = calculate_metrics(actual_test_prices_sma, sma_predictions)

    print("Overall SMA Baseline Metrics:")
    for metric, value in overall_sma_metrics.items():
        print(f"{metric}: {value:.4f}")

    # Calculate and Display SMA Metrics per Cryptocurrency
    print("\nSMA Baseline Metrics per Cryptocurrency:")

    per_crypto_sma_metrics_list = []
    unique_cryptos_in_test = test_df[grouping_column].unique() # Get unique cryptos in the test set

    for crypto_id in unique_cryptos_in_test:
        # Filter data for the current cryptocurrency in the test set
        crypto_test_df = sma_test_predictions_df[sma_test_predictions_df[grouping_column] == crypto_id].copy()

        # Get actual and predicted prices for this crypto
        actual_prices_crypto = crypto_test_df[price_column]
        predicted_prices_crypto = crypto_test_df['SMA_Prediction']

        # Ensure there are enough data points for metric calculation
        if len(actual_prices_crypto) > 1: # Need at least two points for R2/Spearman variance checks
             # Calculate metrics for this cryptocurrency
            crypto_metrics = calculate_metrics(actual_prices_crypto, predicted_prices_crypto)
            crypto_metrics['Crypto'] = crypto_id # Add crypto identifier
            per_crypto_sma_metrics_list.append(crypto_metrics)
        elif len(actual_prices_crypto) > 0: # Handle cases with single point or no variance
            metrics_if_no_variance = calculate_metrics(actual_prices_crypto, predicted_prices_crypto)
            metrics_if_no_variance.update({'Crypto': crypto_id})
            per_crypto_sma_metrics_list.append(metrics_if_no_variance)
        else:
            print(f"Not enough data points in the test set for {crypto_id} to calculate metrics.")
            per_crypto_sma_metrics_list.append({'Crypto': crypto_id, 'MAE': np.nan, 'RMSE': np.nan, 'R²': np.nan, 'MAPE': np.nan, 'Spearman': np.nan, 'Error Std': np.nan})


    # Convert the list of dictionaries to a pandas DataFrame for nice printing
    if per_crypto_sma_metrics_list:
        per_crypto_sma_metrics_df = pd.DataFrame(per_crypto_sma_metrics_list)
        per_crypto_sma_metrics_df.set_index('Crypto', inplace=True)
        per_crypto_sma_metrics_df.sort_values(by='MAE', ascending=True, inplace=True)

        # Select only numeric columns for formatting and print
        numeric_cols = per_crypto_sma_metrics_df.select_dtypes(include=np.number).columns
        formatted_df = per_crypto_sma_metrics_df[numeric_cols].applymap(lambda x: f'{x:.4f}' if pd.notna(x) else '')
        print(formatted_df.to_string())

    else:
        print("No per-cryptocurrency SMA metrics could be calculated.")

else:
    print("Not enough data to calculate overall SMA baseline predictions for the test period.")


# --- Visualization of SMA Baseline Predictions ---
print(f"\nGenerating plots for {len(unique_cryptos_in_test)} cryptocurrencies (SMA Baseline)...")

# Add the SMA predictions to the test_df DataFrame for easier plotting
test_df['Predicted_Price_SMA'] = sma_predictions # Add SMA predictions for plotting

# Determine the layout of the subplots (e.g., 2 columns)
n_cols = 2
n_rows = (len(unique_cryptos_in_test) + n_cols - 1) // n_cols

# Create the figure and subplots
fig, axes = plt.subplots(n_rows, n_cols, figsize=(15, n_rows * 5), squeeze=False)

# Flatten the axes array for easy iteration
axes = axes.flatten()


# Iterate through each cryptocurrency and create a subplot
for i, crypto_id in enumerate(unique_cryptos_in_test):
    ax = axes[i]

    # Filter data for the current cryptocurrency
    crypto_test_df = test_df[test_df[grouping_column] == crypto_id].copy()

    # Get dates, actual prices, and predicted prices for this crypto
    dates = crypto_test_df[date_column]
    actual_prices = crypto_test_df[price_column]
    predicted_prices = crypto_test_df['Predicted_Price_SMA'] # Use SMA predicted price

    # Plot actual and predicted prices
    ax.plot(dates, actual_prices, label='Actual Prices', alpha=0.8)
    ax.plot(dates, predicted_prices, label=f'SMA ({sma_window}-day) Predictions', alpha=0.8, linestyle='--')

    # Set title and labels
    ax.set_title(f'{crypto_id.capitalize()} Price Predictions (SMA Baseline)')
    ax.set_xlabel('Date')
    ax.set_ylabel('Price')
    ax.legend()
    ax.grid(True, alpha=0.6)

# Hide any unused subplots
for j in range(i + 1, len(axes)):
    fig.delaxes(axes[j])

plt.tight_layout()
plt.show()