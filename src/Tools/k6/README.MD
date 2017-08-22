K6 is a stress tool for devs 
===========
Homepage: https://github.com/loadimpact/k6

Latest win64 version can found here
https://github.com/loadimpact/k6/releases/download/v0.17.1/k6-v0.17.1-win64.zip

Installing
==========

Use `install.ps1` script to download and unpack `k6.exe` executable.

Usage
=====


In `VS Code` there is two tasks:
- Default(Ctrl+Shift+B) to run single iteration of `interviews.js` script. Usefull for develop and debug script
- `Big run` will run 400 iterations for 10 agents. Feel free to adjust values

Outside of `VS Code` just use `k6.exe` cli. More details on their doc page: https://docs.k6.io/