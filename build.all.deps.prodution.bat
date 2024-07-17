start cmd /c "TITLE Designer Questionnaire && cd src\UI\WB.UI.Designer && npm install && npm run build && cd .\questionnaire-app && npm install && npm run devb"
call cmd  /c "TITLE hq deps && cd src\UI\WB.UI.Frontend && npm i && npm run build"

