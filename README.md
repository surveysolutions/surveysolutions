5.5 
- .NET Framework 4.6.1 is required    https://www.microsoft.com/en-us/download/details.aspx?id=49982
- PostgreSQL 9.5 is required. Migration described here: http://wiki.wbcapi.org/dev_faq
- Release is compatible with EventStore 3.4.0
- Run utlity QToPlainStore to copy questionnaires to the plain storage. 
  Read side should be valid and not empty. 
  Utility has 2 params: "r" (readside) and "p" (plainstorage).