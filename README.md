# Hebereke Enjoy Edition Extractor

This console application allows the extracting a Famicom ROM of the original game from the [Steam re-release](https://store.steampowered.com/app/2869000).
It also offers the ability to automatically apply a fixed [NES 2.0 header](https://www.nesdev.org/wiki/NES_2.0) to the extracted ROM making it 1:1 equivalent to the ROM cataloged in the [No-Intro set](https://datomatic.no-intro.org/index.php?page=show_record&s=45&n=0942).

## Usage
1. Put "Hebereke_Enjoy_Edition_Extractor.exe" into the same directory as the file "NesHebereke_x64_Release.dll". This file is located in a subdirectory of the games' installation folder. The path should look something like this: `X:\XXX\HEBEREKE\FCHEBEREKE_Data\Plugins\x86_64\`
2. Launch "Hebereke_Enjoy_Edition_Extractor.exe". You should immediately be prompted whether you want to apply a fixed header to the ROM that's going to be extracted or not. Enter either y for yes or n for no.
3. The ROM has now been written to the file "Hebereke (Japan).nes" located within the same directory
