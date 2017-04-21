usage: support <command> [<args>]
These are common Support commands used in various situations:

Health check of Survey Solutions services
usage: support healh-check /path:"<path>"
                    [--all] [--survey-solutions | -ss]
                    [--database-connection | -dbc]
                    [--database-permissions | -dbp]
Options:

<path>
   Physical path to Headquarters website.
   If you are using SurveySolutions installer, by default path to Headquarters website is: C:\\Site

--all
   Run all health checks. All health checks include:
      * Check access to Survey Solutions website
      * Check access to Headquarters database
      * Check permissions to Headquarters database. Check that user which connected to database is owner of that database
-ss
--survey-solutions
   Check access to Survey Solutions website
-dbc
--database-connection
   Check access to Headquarters database
-dbp
--database-permissions
   Check permissions to Headquarters database. Check that user which connected to database is owner of that database


Archive Headquarters log files
usage: support archive-logs /path:"<path>"

Options:

<path>
   Physical path to Headquarters website.
   If you are using SurveySolutions installer, by default path to Headquarters website is: C:\\Site