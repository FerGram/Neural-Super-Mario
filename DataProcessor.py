import pandas as pd
import numpy as np
import os
import re

""" 
Dataset downloaded from https://ped.institutedigitalgames.com/. This script is not really worth reading as
it's more of a footprint of what has been done to achieve the final data processing. It was used to get to
the first stages of the final CSV file. However, it was modified later on to alter that existing processed data
to achieve a better processing. This is a one-use-only script so I didn't put much effort in making it extra clean

The goal with the script is to read all player data in the "All CSV" folder and flatten all the info into one
single row in order to be fed to the neural network. This is how every row ends:
- 5 columns for demographics 
- 3 runs * (6 actions monitored (count of times the action was done in the run) + 1 time invested in run): Total of 21 columns
- 3 result columns of how did the player find the level (engagement, challenge and frustration). This is the variable data
"""

final_gameplay_data = []
final_result_data = []
current_max_length = 0
for (root, dirs, files) in os.walk("All CSV", topdown=True):
    gameplay_pattern = "^[1-9][1-9]?_[ABab]\.csv"
    print(root)
    row_count = 0
    for file in files:

        # Find the gameplay file with
        gameplay_file = re.search(gameplay_pattern, file)


        if gameplay_file is not None:
            full_gameplay_file_path = root + "/" + gameplay_file.string
            full_dem_file_path = root + "/Dem.csv"
            full_result_file_path = root + "/Pref.csv"

            full_gameplay_file_path = full_gameplay_file_path.replace("\\", "/")

            print(full_gameplay_file_path)

            # Read the CSV file
            fileGameDF = pd.read_csv(full_gameplay_file_path, header=None)
            fileDemDF = pd.read_csv(full_dem_file_path, header=None)
            fileResDF = pd.read_csv(full_result_file_path)

            gameplay_row = []
            gameplay_row.append(int(fileDemDF.at[0, 0]))
            gameplay_row.append(int(fileDemDF.at[0, 1]))
            gameplay_row.append(int(fileDemDF.at[0, 2]))
            gameplay_row.append(int(fileDemDF.at[0, 3]))
            gameplay_row.append(int(fileDemDF.at[0, 4]))

            # Append a zero array (7 actions evaluated, 3 runs)
            zero_array = [0] * 7 * 3
            gameplay_row += zero_array

            time_alive = 0
            current_run = 0
            for index, row in fileGameDF.iterrows():

                if row[1] == "030" or row[1] == "330" or row[1] == "430" or row[1] == "031" or row[1] == "331" or row[1] == "431":
                    if row[1] == "030":
                        gameplay_row[5 + 7 * current_run] += 1
                    elif row[1] == "330":
                        gameplay_row[6 + 7 * current_run] += 1
                    elif row[1] == "430":
                        gameplay_row[7 + 7 * current_run] += 1
                    elif row[1] == "031":
                        gameplay_row[8 + 7 * current_run] += 1
                    elif row[1] == "331":
                        gameplay_row[9 + 7 * current_run] += 1
                    elif row[1] == "431":
                        gameplay_row[10 + 7 * current_run] += 1
                elif type(row[1]) is str and str.lower(row[1]) == " level_end ":
                    gameplay_row[11 + 7 * current_run] = int(row[0]) - time_alive
                    time_alive = int(row[0])
                    current_run += 1

            # gameplay_row.append(int(fileResDF.at[row_count, "Engagement"]))
            # gameplay_row.append(int(fileResDF.at[row_count, "Frustration"]))
            # gameplay_row.append(int(fileResDF.at[row_count, "Challenge"]))
            # row_count += 1
            final_gameplay_data.append(np.array(gameplay_row))

            row_idx = int(gameplay_file.string[0]) - 1
            result_row = fileResDF.iloc[row_idx]

            final_result_data.append(np.array(result_row))

            if len(gameplay_row) > current_max_length:
                current_max_length = len(gameplay_row)

            print('--------------------------------')
# The different rows of the final_gameplay_data array are of variable length, we will fill
# the remaining spots with a value of -100 to keep a clean squared matrix.

array_filled = []
# Array with max length full of -100
array_minus_hundred = np.full(current_max_length, -100)

# Fill the rows with -100 up until currentMaxLength
for row in final_gameplay_data:
    if len(row) < current_max_length:
        array_filled.append(np.concatenate([row, array_minus_hundred[len(row):]]))
    else:
        array_filled.append(row)

final_filled_gameplay_data = np.array(array_filled)
df = pd.DataFrame(final_filled_gameplay_data)
df.to_csv('GameplayData.csv', index=False)

df = pd.DataFrame(final_result_data)
df.to_csv('ResultData.csv', index=False)