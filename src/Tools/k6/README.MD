K6 is a stress tool for devs 
===========
Homepage: https://github.com/loadimpact/k6

Installing
==========

Use `install.ps1` script to download and unpack `k6.exe` executable.

Usage
=====

Import questionnaire `K6 tool working questionnaire`, create assignments https://design-dev.mysurvey.solutions/questionnaire/details/f5edcd2f6788491ebd59a01019d67008

In `config.js` provide interviewers and assignments

In `VS Code` there is two tasks:
- Default(Ctrl+Shift+B) to run single iteration of `interviews.js` script. Usefull for develop and debug script
- `Big run` will run 400 iterations for 10 agents. Feel free to adjust values

Outside of `VS Code` just use `k6.exe` cli.

`k6.exe run -u {numberOfUsers} -i {numberOfIterations} interview.js`

More details on their doc page: https://docs.k6.io/
