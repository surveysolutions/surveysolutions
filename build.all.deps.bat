echo Build WB.UI.Headquarters\Dependencies
pushd src\UI\Headquarters\WB.UI.Headquarters\Dependencies
call yarn 
call yarn gulp
popd

echo Build WB.UI.Headquarters\HqApp
pushd src\UI\Headquarters\WB.UI.Headquarters\HqApp
call yarn && call yarn dev
popd


echo Build WB.UI.Designer\questionnaire
pushd src\UI\Designer\WB.UI.Designer\questionnaire
call yarn && call yarn dev
popd
