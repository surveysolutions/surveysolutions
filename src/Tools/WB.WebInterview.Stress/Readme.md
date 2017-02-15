Web Interview stress tool
================================

Usage
-----

By default tool will read `config.yml` from same folder as `.exe`, or configuration file
can be provided as first argument.

Config.yml
----------

Sample config file:

```yml
# Link to web interview start page. It should be not protected by captcha
startUri: https://superhq-dev.mysurvey.solutions/WebInterview/Start/ddb717c84a09420c9001dfb099038f1b$1

## Worker delays specify max amount of time for random generator beetween 0 and delay in ms

# Specify how much time worker should wait while answering next question
answerDelay: 1500

# Spefic how much time worker should work with single interview before starting new
restartWorkersIn: 200000

# max random delay before creating new interview, 
# usefull to make it around 30-40 seconds, so that server is not hammered with create interview tasks
createInterviewDelay: 1000

# how much workers should spam server in parallel
workersCount: 50
```

Caveats
-------

Initial start can be quite long, while workers read questionarie




