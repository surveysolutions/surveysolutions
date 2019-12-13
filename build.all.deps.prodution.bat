start cmd /c "TITLE Designer Questionnaire && cd src\UI\WB.UI.Designer && yarn && yarn gulp --production"
call cmd  /c "TITLE hq deps && cd src\UI\WB.UI.Frontend && yarn && yarn build"

