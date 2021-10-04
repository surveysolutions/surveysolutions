start cmd /c "TITLE Designer Questionnaire && cd src\UI\WB.UI.Designer && npm install && npm run dev"
call cmd  /c "TITLE hq deps && cd src\UI\WB.UI.Frontend && npm install && npm run build"

