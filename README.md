# Blast Wave PPV Optimizer

## Usage Guide
1) Prepare the main Blasting Data folder with the following structure:
   - `Signature Wave\` (numbered files 1.txt, 2.txt, etc.)
   - `Delay Scenario\` (numbered files 1.txt, 2.txt, etc.)
   - `Explosive Weight\` (numbered files 1.txt, 2.txt, etc.)
   - `Simulation Distance\distanceaverage.txt`
   - `Simulation Distance\distancesimulation.txt`

2) Confirm file formats:
   - **Signature Wave**: header contains `Sample Rate : <number> sps`, then three columns (Tran, Vert, Long).
   - **Delay Scenario**: first line is a header (ignored), remaining lines are delay values (numbers).
   - **Explosive Weight**: each line is a weight value (number).
   - **Distance**: each line is a number (double).

3) Open the app, click **Data Directory...**, and select the “Blasting Data” folder.

4) Fill inputs:
   - **Field Constant (B)** (can be negative/positive, must be a number)
   - **Signature Hole Charge (kg)**
   - **Full Blast Duration (ms)**

5) Click **Calculate**.

6) Results:
   - Signature and optimized wave charts appear.
   - PPV table per scenario is populated.
   - Labels show the best PPV for each component and PVS.
   - Output files are written in the same folder:
     `result_Tran.txt`, `result_Vert.txt`, `result_Long.txt`, `result_PVS.txt`.

Notes:
- All files must be sequentially numbered starting at 1.
- Sample rates across signature files must match.

## Overview
Windows Forms application to compute and optimize PPV (Peak Particle Velocity) based on signature wave data, delay scenarios, weights, and distances. It plots Tran/Vert/Long and PVS components and writes optimization outputs to files.

## Formula
USBM reference:
- v = K (D / sqrt(Qmax))^-b (Duvall & Petkof)

The implementation uses a ratio against the signature wave, so scale is computed as a PPV ratio against the signature.

## Output
Result files are written to the selected input folder:
- `result_Tran.txt`
- `result_Vert.txt`
- `result_Long.txt`
- `result_PVS.txt`

## Build & Publish
Publish ke folder `artifacts\`:
```powershell
.\publish.ps1
```

## Project Structure (core)
- `Form1.cs`: UI and main flow.
- `DataLoader.cs`: input reading and validation.
- `FileParsing.cs`: signature header and wave parsing.
- `WaveCalculator.cs`: wave and PPV computation.
- `ResultWriter.cs`: result file output.
