#!/bin/zsh
#echo "Hello      World"
cd src/UI/WB.UI.Designer 
npm i
npm run dev
cd ./questionnaire
npm i
npm run devb
cd ../../WB.UI.Frontend
npm i
npm run dev
