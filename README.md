# berserk-manga-scraper

`Release.zip` contains binaries pre-compiled on `arch-linux` for `net5.0`, `net6.0` and `net7.0`, it should work for any system with propper `dotnet-runtime`

Scrapes off of https://readberserk.com/ big kudos to them!

## How to run
There are 2 ways:

### Via precompiled Releases.zip
1. Check what version of dotnet you have using `dotnet --info` in powershell/terminal
  1. If you don't have any download and install it off Microsoft site  
2. Download and unpack `Releases.zip`
3. Go into folder with desired version of dotnet (inside unpacked archive)
4. run `dotnet ./img-download.dll`

### Using source
1. Download source
2. run `dotnet run` in main directory
