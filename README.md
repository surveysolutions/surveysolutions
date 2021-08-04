
# Notice

Reading, cloning, forking, or otherwise compiling the source code in this repository is an explicit acknowledgement of license terms outlined in the [license](https://github.com/surveysolutions/surveysolutions/blob/master/LICENSE.md) file.
# Overview

Survey Solutions is a survey management and data collection system developed by the World Bank. The software is used worldwide by the National Statistical Offices, Central Banks, Non-Government Organizations and universities to collect and manage surveys of households, individuals, enterprises (firms/establishments), infrastructure (schools, hospitals, etc) and communities. It has been used to conduct censuses, household income and expenditure surveys, labor force surveys, price surveys, and other types of data collection operations.

# Documentation
Comprehensive documentation is available at https://docs.mysurvey.solutions. Deployment instructions for production instances can be also found [there](https://docs.mysurvey.solutions/headquarters/config/server-setup/). To see the history of previous releases please take a look at our [release notes](https://docs.mysurvey.solutions/release-notes/).

This repository contains source code for the following major components of Survey solutions:

## Web applications

1. **Designer** - place for designing questionnaires. Supports sharing and collaboration between authors of questionnaires.
1. **Web Tester** - application used to quickly test questionnaires using web browser. Supports recording of scenarios to ease testing of large questionnaires.
1. **Headquarters** - used for survey management and data collection. Surveys as central storage of collected interviews for both CAPI and CAWI modes.
1. **Export service** - backend service that has no UI. Used by Headquarters application to generate export files. Supports exporting to various statistical packages.

## Android applications

1. **Interviewer** - data collection tool used by field workers to conduct interviews.
1. **Supervisor** - can be used as temporary storage of interviews in the areas where there is no internet connectivity. Allows to receive interviews and distribute work for interviewers with [nearby communication](https://developers.google.com/nearby).
1. **Tester** - same as interviewer application, but works directly with Designer application. Should be used to verify performance and usability of questionnaire during design time.

# Contributing
All contributions, bug reports, documentation improvements, helping with translations, and new feature ideas are welcome.

If you are interested in making modifications to the code, please take a look at local building and development environment description in the [contributing](https://github.com/surveysolutions/surveysolutions/blob/master/CONTRIBUTING.md) file.

Please reach out to the team first to discuss your ideas if you think that you'd like to contribute your changes and/or additions to the product.

# Security

Please read our [security guide](https://github.com/surveysolutions/surveysolutions/blob/master/SECURITY.md) on how we protect your survey data, or if you wish to report a security issue.