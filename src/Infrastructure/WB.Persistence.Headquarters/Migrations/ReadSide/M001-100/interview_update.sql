-- DROP TYPE readside.interviewvalidation;
CREATE TYPE readside.interviewvalidation AS
   (id uuid,
    rostervector integer[],
    validations integer[]);

-- DROP TYPE readside.interviewidentity;
CREATE TYPE readside.interviewidentity AS
   (id uuid,
    rostervector integer[]);

-- DROP TYPE readside.interviewanswer;
CREATE TYPE readside.interviewanswer AS
   (id uuid,
    rostervector integer[],
    asstring text,
    asint integer,
    aslong bigint,
    asdouble double precision,
    asdatetime timestamp without time zone,
    aslist jsonb,
    asintarray integer[],
    asintmatrix jsonb,
    asgps jsonb,
    asbool boolean,
    asyesno jsonb,
    asaudio jsonb,
    asarea jsonb);

-- DROP FUNCTION readside.interview_update(id uuid,removed readside.interviewidentity[],enabled readside.interviewidentity[],disabled readside.interviewidentity[],validations readside.interviewvalidation[],readonlyentities readside.interviewidentity[],answers readside.interviewanswer[]);
CREATE OR REPLACE FUNCTION readside.interview_update(id uuid,
    removed readside.interviewidentity[],
    enabled readside.interviewidentity[],
    disabled readside.interviewidentity[],
    validations readside.interviewvalidation[],
    readonlyentities readside.interviewidentity[],
    answers readside.interviewanswer[])
  RETURNS void AS
$BODY$
    DECLARE
  entity readside.interviewidentity;
  validation readside.interviewvalidation;
  answer readside.interviewanswer;
  BEGIN
  FOREACH entity IN ARRAY removed
  LOOP
  DELETE FROM readside.interviews WHERE interviewid = id AND entityid = entity.id AND rostervector = entity.rostervector;
  END LOOP;
    FOREACH entity IN ARRAY enabled
  LOOP
  INSERT INTO readside.interviews (interviewid, entityid, rostervector, isenabled)
                VALUES(id, entity.id, entity.rostervector, true)
                ON CONFLICT ON CONSTRAINT pk_interviews
                DO UPDATE SET
                isenabled = true;
  END LOOP;
  FOREACH entity IN ARRAY disabled
  LOOP
  INSERT INTO readside.interviews (interviewid, entityid, rostervector, isenabled)
                VALUES(id, entity.id, entity.rostervector, false)
                ON CONFLICT ON CONSTRAINT pk_interviews
                DO UPDATE SET
                isenabled = false;
  END LOOP;
  FOREACH entity IN ARRAY readonlyentities
  LOOP
  INSERT INTO readside.interviews (interviewid, entityid, rostervector, isreadonly)
                VALUES(id, entity.id, entity.rostervector, true)
                ON CONFLICT ON CONSTRAINT pk_interviews
                DO UPDATE SET
                isreadonly = true;
  END LOOP;
  FOREACH validation IN ARRAY validations
  LOOP
  INSERT INTO readside.interviews (interviewid, entityid, rostervector, invalidvalidations)
                VALUES(id, validation.id, validation.rostervector, validation.validations)
                ON CONFLICT ON CONSTRAINT pk_interviews
                DO UPDATE SET
                invalidvalidations = validation.validations;
  END LOOP;
  FOREACH answer IN ARRAY answers
  LOOP
  INSERT INTO readside.interviews (interviewid, entityid, rostervector, asint, asdouble, aslong, asdatetime, asstring, aslist, asintarray, asintmatrix, asgps, asbool, asyesno, asaudio, asarea) 
                            VALUES(id, answer.id, answer.rostervector, answer.asint, answer.asdouble, answer.aslong, answer.asdatetime, answer.asstring, answer.aslist, answer.asintarray, answer.asintmatrix, answer.asgps, answer.asbool, answer.asyesno, answer.asaudio, answer.asarea)
                ON CONFLICT ON CONSTRAINT pk_interviews
                DO UPDATE SET
                asstring = answer.asstring,
		asint = answer.asint ,
		aslong = answer.aslong,
		asdouble = answer.asdouble,
		asdatetime = answer.asdatetime,
		aslist = answer.aslist,
		asintarray = answer.asintArray,
		asintmatrix = answer.asintmatrix,
		asgps = answer.asgps,
		asbool = answer.asbool,
		asyesno = answer.asyesno,
		asaudio = answer.asaudio,
		asarea = answer.asarea;
  END LOOP;
    END;
    $BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
