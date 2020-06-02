- **switch to linux containers** or enable docker setting: `  "experimental": true`
- docker\build.ps1 will build export and headquarters images locally
  - for export it will run test and will create image only if successful

- run docker-compose up to bring up headquarters
  - database creates local volume so data are persisted
  - headquarters will be exposed to 5000, can be changed in .env file
