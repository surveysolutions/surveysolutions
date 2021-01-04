
CREATE TABLE interview_geo_answers (
                                                  id integer NOT NULL,
                                                  interview_id integer NOT NULL,
                                                  questionid uuid NOT NULL,
                                                  rostervector text NOT NULL,
                                                  latitude double precision NOT NULL,
                                                  longitude double precision NOT NULL,
                                                  "timestamp" text,
                                                  isenabled boolean DEFAULT true NOT NULL
);


--
-- Name: __interview_geo_answers_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE __interview_geo_answers_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: __interview_geo_answers_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE __interview_geo_answers_id_seq OWNED BY interview_geo_answers.id;


--
-- Name: appsettings; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE appsettings (
                                        id text NOT NULL,
                                        value jsonb NOT NULL
);


--
-- Name: assemblyinfos; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE assemblyinfos (
                                          id character varying(255) NOT NULL,
                                          creationdate timestamp without time zone,
                                          content bytea
);


--
-- Name: assignment_id_sequence; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE assignment_id_sequence
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: assignments; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE assignments (
                                        publickey uuid NOT NULL,
                                        id integer NOT NULL,
                                        responsibleid uuid NOT NULL,
                                        quantity integer,
                                        archived boolean NOT NULL,
                                        createdatutc timestamp without time zone NOT NULL,
                                        updatedatutc timestamp without time zone NOT NULL,
                                        questionnaireid uuid NOT NULL,
                                        questionnaireversion integer NOT NULL,
                                        questionnaire character varying(255),
                                        answers jsonb,
                                        protectedvariables jsonb,
                                        receivedbytabletatutc timestamp without time zone,
                                        audiorecording boolean DEFAULT false NOT NULL,
                                        email text,
                                        password text,
                                        webmode boolean,
                                        comments text
);


--
-- Name: assignmentsidentifyinganswers; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE assignmentsidentifyinganswers (
                                                          assignmentid integer NOT NULL,
                                                          "position" integer NOT NULL,
                                                          questionid uuid NOT NULL,
                                                          answer text,
                                                          answerasstring text,
                                                          rostervector integer[] NOT NULL
);


--
-- Name: assignmentsimportprocess; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE assignmentsimportprocess (
                                                     id integer NOT NULL,
                                                     questionnaireid text NOT NULL,
                                                     filename text NOT NULL,
                                                     totalcount integer NOT NULL,
                                                     responsible text NOT NULL,
                                                     starteddate timestamp without time zone NOT NULL,
                                                     status integer,
                                                     assignedto uuid NOT NULL
);


--
-- Name: assignmentsimportprocess_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE assignmentsimportprocess_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: assignmentsimportprocess_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE assignmentsimportprocess_id_seq OWNED BY assignmentsimportprocess.id;


--
-- Name: assignmenttoimport; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE assignmenttoimport (
                                               id integer NOT NULL,
                                               interviewer uuid,
                                               supervisor uuid,
                                               quantity integer,
                                               answers jsonb,
                                               verified boolean DEFAULT false NOT NULL,
                                               error text,
                                               protectedvariables jsonb,
                                               email text,
                                               password text,
                                               webmode boolean,
                                               isaudiorecordingenabled boolean,
                                               headquarters uuid,
                                               comments text
);


--
-- Name: assignmenttoimport_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE assignmenttoimport_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: assignmenttoimport_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE assignmenttoimport_id_seq OWNED BY assignmenttoimport.id;


--
-- Name: attachmentcontents; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE attachmentcontents (
                                               id character varying(255) NOT NULL,
                                               contenttype text,
                                               content bytea,
                                               filename text
);


--
-- Name: audioauditfiles; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE audioauditfiles (
                                            id text NOT NULL,
                                            interviewid uuid NOT NULL,
                                            filename text NOT NULL,
                                            contenttype text,
                                            data bytea
);


--
-- Name: audiofiles; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE audiofiles (
                                       id text NOT NULL,
                                       interviewid uuid NOT NULL,
                                       filename text NOT NULL,
                                       data bytea NOT NULL,
                                       contenttype text
);


--
-- Name: auditlogrecords; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE auditlogrecords (
                                            id integer NOT NULL,
                                            recordid integer NOT NULL,
                                            responsibleid uuid,
                                            responsiblename text,
                                            type text NOT NULL,
                                            "time" timestamp without time zone NOT NULL,
                                            timeutc timestamp without time zone NOT NULL,
                                            payload text NOT NULL
);


--
-- Name: auditlogrecords_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE auditlogrecords_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: auditlogrecords_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE auditlogrecords_id_seq OWNED BY auditlogrecords.id;


--
-- Name: brokeninterviewpackages; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE brokeninterviewpackages (
                                                    id integer NOT NULL,
                                                    interviewid uuid,
                                                    questionnaireid uuid,
                                                    questionnaireversion bigint,
                                                    responsibleid uuid,
                                                    interviewstatus integer,
                                                    iscensusinterview boolean,
                                                    incomingdate timestamp without time zone,
                                                    events text,
                                                    processingdate timestamp without time zone,
                                                    exceptiontype text,
                                                    exceptionmessage text,
                                                    exceptionstacktrace text,
                                                    packagesize bigint,
                                                    reprocessattemptscount integer NOT NULL,
                                                    interviewkey character varying(12)
);


--
-- Name: brokeninterviewpackages_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE brokeninterviewpackages_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: brokeninterviewpackages_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE brokeninterviewpackages_id_seq OWNED BY brokeninterviewpackages.id;


--
-- Name: commentaries; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE commentaries (
                                         commentsequence integer,
                                         originatorname text,
                                         originatoruserid uuid,
                                         originatorrole integer,
                                         "timestamp" timestamp without time zone,
                                         variable text,
                                         roster text,
                                         rostervector numeric[],
                                         comment text,
                                         summary_id integer NOT NULL,
                                         id integer NOT NULL
);


--
-- Name: commentaries_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE commentaries_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: commentaries_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE commentaries_id_seq OWNED BY commentaries.id;


--
-- Name: completedemailrecords; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE completedemailrecords (
                                                  interviewid uuid NOT NULL,
                                                  requesttime timestamp without time zone NOT NULL,
                                                  failedcount integer NOT NULL
);


--
-- Name: cumulativereportstatuschanges; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE cumulativereportstatuschanges (
                                                          entryid character varying(255) NOT NULL,
                                                          date timestamp without time zone,
                                                          status integer,
                                                          changevalue integer,
                                                          questionnaireidentity text,
                                                          interviewid uuid,
                                                          eventsequence integer
);


--
-- Name: deviceinfos; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE deviceinfos (
                                        id integer NOT NULL,
                                        "User" character varying(255),
                                        date timestamp without time zone,
                                        deviceid text,
                                        userid character varying(255)
);


--
-- Name: devicesyncinfo; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE devicesyncinfo (
                                           "Id" integer NOT NULL,
                                           "SyncDate" timestamp without time zone NOT NULL,
                                           "InterviewerId" uuid NOT NULL,
                                           "DeviceId" text,
                                           "DeviceModel" text,
                                           "DeviceType" text,
                                           "DeviceDate" timestamp without time zone NOT NULL,
                                           "DeviceLocationLat" double precision,
                                           "DeviceLocationLong" double precision,
                                           "DeviceLanguage" text,
                                           "DeviceManufacturer" text,
                                           "DeviceBuildNumber" text,
                                           "DeviceSerialNumber" text,
                                           "AndroidVersion" text,
                                           "AndroidSdkVersion" integer NOT NULL,
                                           "AndroidSdkVersionName" text,
                                           "AppVersion" text,
                                           "AppBuildVersion" integer NOT NULL,
                                           "LastAppUpdatedDate" timestamp without time zone NOT NULL,
                                           "NetworkType" text,
                                           "NetworkSubType" text,
                                           "MobileOperator" text,
                                           "AppOrientation" text,
                                           "BatteryPowerSource" text,
                                           "BatteryChargePercent" integer NOT NULL,
                                           "IsPowerInSaveMode" boolean NOT NULL,
                                           "MobileSignalStrength" integer NOT NULL,
                                           "StorageTotalInBytes" bigint NOT NULL,
                                           "StorageFreeInBytes" bigint NOT NULL,
                                           "RAMTotalInBytes" bigint NOT NULL,
                                           "RAMFreeInBytes" bigint NOT NULL,
                                           "DBSizeInfo" bigint NOT NULL,
                                           "NumberOfStartedInterviews" integer NOT NULL,
                                           "StatisticsId" integer
);


--
-- Name: devicesyncinfo_Id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE "devicesyncinfo_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: devicesyncinfo_Id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE "devicesyncinfo_Id_seq" OWNED BY devicesyncinfo."Id";


--
-- Name: devicesyncstatistics; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE devicesyncstatistics (
                                                 "Id" integer NOT NULL,
                                                 "UploadedInterviewsCount" integer NOT NULL,
                                                 "DownloadedInterviewsCount" integer NOT NULL,
                                                 "DownloadedQuestionnairesCount" integer NOT NULL,
                                                 "RejectedInterviewsOnDeviceCount" integer NOT NULL,
                                                 "NewInterviewsOnDeviceCount" integer NOT NULL,
                                                 "NewAssignmentsCount" integer NOT NULL,
                                                 "RemovedAssignmentsCount" integer NOT NULL,
                                                 "RemovedInterviewsCount" integer,
                                                 "AssignmentsOnDeviceCount" integer NOT NULL,
                                                 "TotalUploadedBytes" bigint NOT NULL,
                                                 "TotalDownloadedBytes" bigint NOT NULL,
                                                 "TotalConnectionSpeed" double precision NOT NULL,
                                                 "TotalSyncDuration" interval(6) NOT NULL,
                                                 "SyncFinishDate" timestamp without time zone NOT NULL
);


--
-- Name: devicesyncstatistics_Id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE "devicesyncstatistics_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: devicesyncstatistics_Id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE "devicesyncstatistics_Id_seq" OWNED BY devicesyncstatistics."Id";


--
-- Name: globalsequence; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE globalsequence
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: events; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE events (
                                   id uuid NOT NULL,
                                   origin text,
                                   "timestamp" timestamp without time zone NOT NULL,
                                   eventsourceid uuid NOT NULL,
                                   globalsequence integer DEFAULT nextval('globalsequence'::regclass) NOT NULL,
                                   value jsonb NOT NULL,
                                   eventsequence integer NOT NULL,
                                   eventtype text NOT NULL
);


--
-- Name: featuredquestions; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE featuredquestions (
                                              questionnaireid character varying(255) NOT NULL,
                                              "position" integer NOT NULL,
                                              id uuid,
                                              title text,
                                              caption text
);


--
-- Name: hibernate_unique_key; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE hibernate_unique_key (
    next_hi integer NOT NULL
);

INSERT INTO hibernate_unique_key (next_hi) VALUES(0);

--
-- Name: identifyingentityvalue; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE identifyingentityvalue (
                                                   id integer NOT NULL,
                                                   value text,
                                                   interview_id integer,
                                                   "position" integer,
                                                   entity_id integer,
                                                   answer_code integer,
                                                   value_lower_case text
);


--
-- Name: interviewcommentedstatuses; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE interviewcommentedstatuses (
                                                       "position" integer NOT NULL,
                                                       id uuid NOT NULL,
                                                       interviewername text,
                                                       interviewerid uuid,
                                                       supervisorid uuid,
                                                       supervisorname text,
                                                       statuschangeoriginatorid uuid,
                                                       statuschangeoriginatorname text,
                                                       statuschangeoriginatorrole integer,
                                                       status integer,
                                                       "timestamp" timestamp without time zone,
                                                       timespanwithpreviousstatus bigint,
                                                       comment text,
                                                       interview_id integer NOT NULL
);


--
-- Name: interviewflags; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE interviewflags (
                                           interviewid character(255) NOT NULL,
                                           questionidentity text NOT NULL
);


--
-- Name: interviewpackages; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE interviewpackages (
                                              id integer NOT NULL,
                                              interviewid uuid,
                                              questionnaireid uuid,
                                              questionnaireversion bigint,
                                              responsibleid uuid,
                                              interviewstatus integer,
                                              iscensusinterview boolean,
                                              incomingdate timestamp without time zone,
                                              events text,
                                              processattemptscount integer NOT NULL
);


--
-- Name: interviewpackages_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE interviewpackages_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: interviewpackages_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE interviewpackages_id_seq OWNED BY interviewpackages.id;


--
-- Name: interviewsummaries; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE interviewsummaries (
                                               summaryid character varying(255) NOT NULL,
                                               interviewid uuid,
                                               questionnairetitle text,
                                               responsiblename text,
                                               teamleadid uuid NOT NULL,
                                               teamleadname text,
                                               responsiblerole integer,
                                               updatedate timestamp without time zone,
                                               wasrejectedbysupervisor boolean,
                                               wascreatedonclient boolean,
                                               questionnaireid uuid,
                                               questionnaireversion bigint,
                                               responsibleid uuid,
                                               status integer,
                                               isassignedtointerviewer boolean NOT NULL,
                                               key text,
                                               assignmentid integer,
                                               clientkey character varying(12),
                                               questionnaireidentity text,
                                               wascompleted boolean DEFAULT false NOT NULL,
                                               interviewduration bigint,
                                               lastresumeeventutctimestamp timestamp without time zone,
                                               errorscount integer DEFAULT 0 NOT NULL,
                                               id integer NOT NULL,
                                               createddate timestamp without time zone,
                                               firstanswerdate timestamp without time zone,
                                               firstinterviewerid uuid,
                                               firstinterviewername text,
                                               firstsupervisorid uuid,
                                               firstsupervisorname text,
                                               hasresolvedcomments boolean DEFAULT false NOT NULL,
                                               responsible_name_lower_case text,
                                               teamlead_name_lower_case text,
                                               questionnaire_variable text NOT NULL,
                                               hassmallsubstitutions boolean DEFAULT false NOT NULL,
                                               receivedbyintervieweratutc timestamp without time zone,
                                               not_answered_count integer
);


--
-- Name: interviewsummaries_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE interviewsummaries_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: interviewsummaries_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE interviewsummaries_id_seq OWNED BY interviewsummaries.id;


--
-- Name: invitations; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE invitations (
                                        id integer NOT NULL,
                                        assignmentid integer NOT NULL,
                                        interviewid text,
                                        token text,
                                        resumepassword text,
                                        sentonutc timestamp without time zone,
                                        invitationemailid text,
                                        lastremindersentonutc timestamp without time zone,
                                        lastreminderemailid text,
                                        numberofreminderssent text,
                                        last_rejected_interview_email_id text,
                                        last_rejected_status_position integer
);


--
-- Name: invitations_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE invitations_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: invitations_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE invitations_id_seq OWNED BY invitations.id;


--
-- Name: mapbrowseitems; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE mapbrowseitems (
                                           id character varying(255) NOT NULL,
                                           size bigint,
                                           importdate timestamp without time zone,
                                           filename text,
                                           wkid bigint,
                                           xmaxval double precision,
                                           xminval double precision,
                                           ymaxval double precision,
                                           yminval double precision,
                                           maxscale double precision,
                                           minscale double precision
);

--
-- Name: questionnaire_entities; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE questionnaire_entities (
                                                   id integer NOT NULL,
                                                   questionnaireidentity character varying(255) NOT NULL,
                                                   entityid uuid NOT NULL,
                                                   parentid uuid,
                                                   question_type integer,
                                                   featured boolean,
                                                   question_scope integer,
                                                   entity_type integer,
                                                   is_filtered_combobox boolean DEFAULT false NOT NULL,
                                                   linked_to_question_id uuid,
                                                   linked_to_roster_id uuid,
                                                   stata_export_caption text,
                                                   variable_label text,
                                                   question_text text,
                                                   cascade_from_question_id uuid
);


--
-- Name: questionnaire_entities_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE questionnaire_entities_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: questionnaire_entities_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE questionnaire_entities_id_seq OWNED BY questionnaire_entities.id;


--
-- Name: questionnairebackups; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE questionnairebackups (
                                                 id text NOT NULL,
                                                 value json NOT NULL
);


--
-- Name: questionnairebrowseitems; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE questionnairebrowseitems (
                                                     id character varying(255) NOT NULL,
                                                     creationdate timestamp without time zone,
                                                     questionnaireid uuid,
                                                     version bigint,
                                                     lastentrydate timestamp without time zone,
                                                     title text,
                                                     ispublic boolean,
                                                     createdby uuid,
                                                     isdeleted boolean,
                                                     allowcensusmode boolean,
                                                     disabled boolean,
                                                     questionnairecontentversion bigint,
                                                     importdate timestamp without time zone,
                                                     allowassignments boolean NOT NULL,
                                                     allowexportvariables boolean NOT NULL,
                                                     variable text,
                                                     disabledby uuid,
                                                     isaudiorecordingenabled boolean DEFAULT false NOT NULL,
                                                     comment text,
                                                     importedby uuid
);


--
-- Name: questionnairedocuments; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE questionnairedocuments (
                                                   id text NOT NULL,
                                                   value json NOT NULL
);


--
-- Name: questionnairelookuptables; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE questionnairelookuptables (
                                                      id text NOT NULL,
                                                      value json NOT NULL
);


--
-- Name: questionnairepdfs; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE questionnairepdfs (
                                              id text NOT NULL,
                                              value json NOT NULL
);


--
-- Name: receivedpackagelogentries; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE receivedpackagelogentries (
                                                      id integer NOT NULL,
                                                      firsteventid uuid NOT NULL,
                                                      lasteventid uuid NOT NULL,
                                                      firsteventtimestamp timestamp without time zone NOT NULL,
                                                      lasteventtimestamp timestamp without time zone NOT NULL
);


--
-- Name: receivedpackagelogentries_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE receivedpackagelogentries_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: receivedpackagelogentries_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE receivedpackagelogentries_id_seq OWNED BY receivedpackagelogentries.id;


--
-- Name: report_statistics; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE report_statistics (
                                              id integer NOT NULL,
                                              interview_id integer NOT NULL,
                                              entity_id integer NOT NULL,
                                              rostervector text NOT NULL,
                                              answer bigint[] NOT NULL,
                                              type smallint DEFAULT 0 NOT NULL,
                                              is_enabled boolean DEFAULT true NOT NULL
);


--
-- Name: report_statistics_categorical; Type: VIEW; Schema: ws_primary; Owner: -
--

CREATE VIEW report_statistics_categorical AS
SELECT a.interview_id,
       a.entity_id,
       a.rostervector,
       ans.ans AS answer
FROM report_statistics a,
     LATERAL unnest(a.answer) ans(ans)
WHERE ((a.type = 0) AND (a.is_enabled = true));


--
-- Name: report_statistics_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE report_statistics_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: report_statistics_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE report_statistics_id_seq OWNED BY report_statistics.id;


--
-- Name: report_statistics_numeric; Type: VIEW; Schema: ws_primary; Owner: -
--

CREATE VIEW report_statistics_numeric AS
SELECT a.interview_id,
       a.entity_id,
       a.rostervector,
       ans.ans AS answer
FROM report_statistics a,
     LATERAL unnest(a.answer) ans(ans)
WHERE ((a.type = 1) AND (a.is_enabled = true));


--
-- Name: reusablecategoricaloptions; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE reusablecategoricaloptions (
                                                       id integer NOT NULL,
                                                       questionnaireid uuid NOT NULL,
                                                       questionnaireversion integer NOT NULL,
                                                       categoriesid uuid NOT NULL,
                                                       sortindex integer NOT NULL,
                                                       parentvalue integer,
                                                       value integer NOT NULL,
                                                       text text NOT NULL
);


--
-- Name: reusablecategoricaloptions_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE reusablecategoricaloptions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: reusablecategoricaloptions_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE reusablecategoricaloptions_id_seq OWNED BY reusablecategoricaloptions.id;


--
-- Name: synchronizationlog; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE synchronizationlog (
                                               id integer NOT NULL,
                                               interviewerid uuid,
                                               interviewername text,
                                               deviceid text,
                                               logdate timestamp without time zone,
                                               type integer,
                                               log text,
                                               interviewid uuid,
                                               actionexceptionmessage text,
                                               actionexceptiontype text
);


--
-- Name: systemlog; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE systemlog (
                                      id integer NOT NULL,
                                      type integer NOT NULL,
                                      logdate timestamp without time zone NOT NULL,
                                      userid uuid,
                                      username text,
                                      log text NOT NULL
);


--
-- Name: systemlog_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE systemlog_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: systemlog_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE systemlog_id_seq OWNED BY systemlog.id;


--
-- Name: tablet_logs; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE tablet_logs (
                                        id integer NOT NULL,
                                        device_id text,
                                        user_name text,
                                        content bytea NOT NULL,
                                        receive_date_utc timestamp without time zone NOT NULL
);


--
-- Name: tablet_logs_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE tablet_logs_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: tablet_logs_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE tablet_logs_id_seq OWNED BY tablet_logs.id;


--
-- Name: timespanbetweenstatuses; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE timespanbetweenstatuses (
                                                    id integer NOT NULL,
                                                    supervisorid uuid,
                                                    supervisorname text,
                                                    interviewerid uuid,
                                                    interviewername text,
                                                    beginstatus integer,
                                                    endstatus integer,
                                                    endstatustimestamp timestamp without time zone,
                                                    timespan bigint,
                                                    interviewstatustimespans character varying(255),
                                                    interview_id integer NOT NULL
);


--
-- Name: translationinstances; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE translationinstances (
                                                 id integer NOT NULL,
                                                 questionnaireid uuid NOT NULL,
                                                 type integer NOT NULL,
                                                 questionnaireversion bigint NOT NULL,
                                                 questionnaireentityid uuid NOT NULL,
                                                 translationindex text,
                                                 translationid uuid NOT NULL,
                                                 value text NOT NULL
);


--
-- Name: usermaps; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE usermaps (
                                     id integer NOT NULL,
                                     map character varying(255) NOT NULL,
                                     username character varying(255) NOT NULL
);


--
-- Name: usersimportprocess; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE usersimportprocess (
                                               id integer NOT NULL,
                                               filename text NOT NULL,
                                               supervisorscount integer NOT NULL,
                                               interviewerscount integer NOT NULL,
                                               responsible text NOT NULL,
                                               starteddate timestamp without time zone NOT NULL
);


--
-- Name: usersimportprocess_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE usersimportprocess_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: usersimportprocess_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE usersimportprocess_id_seq OWNED BY usersimportprocess.id;


--
-- Name: usertoimport; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE usertoimport (
                                         id integer NOT NULL,
                                         login text NOT NULL,
                                         email text,
                                         fullname text,
                                         password text NOT NULL,
                                         phonenumber text,
                                         role text NOT NULL,
                                         supervisor text
);


--
-- Name: usertoimport_id_seq; Type: SEQUENCE; Schema: ws_primary; Owner: -
--

CREATE SEQUENCE usertoimport_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: usertoimport_id_seq; Type: SEQUENCE OWNED BY; Schema: ws_primary; Owner: -
--

ALTER SEQUENCE usertoimport_id_seq OWNED BY usertoimport.id;


--
-- Name: webinterviewconfigs; Type: TABLE; Schema: ws_primary; Owner: -
--

CREATE TABLE webinterviewconfigs (
                                                id text NOT NULL,
                                                value json NOT NULL
);


--
-- Name: assignmentsimportprocess id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY assignmentsimportprocess ALTER COLUMN id SET DEFAULT nextval('assignmentsimportprocess_id_seq'::regclass);


--
-- Name: assignmenttoimport id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY assignmenttoimport ALTER COLUMN id SET DEFAULT nextval('assignmenttoimport_id_seq'::regclass);


--
-- Name: auditlogrecords id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY auditlogrecords ALTER COLUMN id SET DEFAULT nextval('auditlogrecords_id_seq'::regclass);


--
-- Name: brokeninterviewpackages id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY brokeninterviewpackages ALTER COLUMN id SET DEFAULT nextval('brokeninterviewpackages_id_seq'::regclass);


--
-- Name: commentaries id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY commentaries ALTER COLUMN id SET DEFAULT nextval('commentaries_id_seq'::regclass);


--
-- Name: devicesyncinfo Id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY devicesyncinfo ALTER COLUMN "Id" SET DEFAULT nextval('"devicesyncinfo_Id_seq"'::regclass);


--
-- Name: devicesyncstatistics Id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY devicesyncstatistics ALTER COLUMN "Id" SET DEFAULT nextval('"devicesyncstatistics_Id_seq"'::regclass);


--
-- Name: interview_geo_answers id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY interview_geo_answers ALTER COLUMN id SET DEFAULT nextval('__interview_geo_answers_id_seq'::regclass);


--
-- Name: interviewpackages id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY interviewpackages ALTER COLUMN id SET DEFAULT nextval('interviewpackages_id_seq'::regclass);


--
-- Name: interviewsummaries id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY interviewsummaries ALTER COLUMN id SET DEFAULT nextval('interviewsummaries_id_seq'::regclass);


--
-- Name: invitations id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY invitations ALTER COLUMN id SET DEFAULT nextval('invitations_id_seq'::regclass);


--
-- Name: questionnaire_entities id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY questionnaire_entities ALTER COLUMN id SET DEFAULT nextval('questionnaire_entities_id_seq'::regclass);


--
-- Name: receivedpackagelogentries id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY receivedpackagelogentries ALTER COLUMN id SET DEFAULT nextval('receivedpackagelogentries_id_seq'::regclass);


--
-- Name: report_statistics id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY report_statistics ALTER COLUMN id SET DEFAULT nextval('report_statistics_id_seq'::regclass);


--
-- Name: reusablecategoricaloptions id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY reusablecategoricaloptions ALTER COLUMN id SET DEFAULT nextval('reusablecategoricaloptions_id_seq'::regclass);


--
-- Name: systemlog id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY systemlog ALTER COLUMN id SET DEFAULT nextval('systemlog_id_seq'::regclass);


--
-- Name: tablet_logs id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY tablet_logs ALTER COLUMN id SET DEFAULT nextval('tablet_logs_id_seq'::regclass);


--
-- Name: usersimportprocess id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY usersimportprocess ALTER COLUMN id SET DEFAULT nextval('usersimportprocess_id_seq'::regclass);


--
-- Name: usertoimport id; Type: DEFAULT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY usertoimport ALTER COLUMN id SET DEFAULT nextval('usertoimport_id_seq'::regclass);


--
-- Name: identifyingentityvalue PK_answerstofeaturedquestions; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY identifyingentityvalue
    ADD CONSTRAINT "PK_answerstofeaturedquestions" PRIMARY KEY (id);


--
-- Name: assemblyinfos PK_assemblyinfos; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY assemblyinfos
    ADD CONSTRAINT "PK_assemblyinfos" PRIMARY KEY (id);


--
-- Name: assignments PK_assignments; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY assignments
    ADD CONSTRAINT "PK_assignments" PRIMARY KEY (publickey);


--
-- Name: assignmentsimportprocess PK_assignmentsimportprocess; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY assignmentsimportprocess
    ADD CONSTRAINT "PK_assignmentsimportprocess" PRIMARY KEY (id);


--
-- Name: assignmenttoimport PK_assignmenttoimport; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY assignmenttoimport
    ADD CONSTRAINT "PK_assignmenttoimport" PRIMARY KEY (id);


--
-- Name: attachmentcontents PK_attachmentcontents; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY attachmentcontents
    ADD CONSTRAINT "PK_attachmentcontents" PRIMARY KEY (id);


--
-- Name: audioauditfiles PK_audioauditfiles; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY audioauditfiles
    ADD CONSTRAINT "PK_audioauditfiles" PRIMARY KEY (id);


--
-- Name: audiofiles PK_audiofiles; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY audiofiles
    ADD CONSTRAINT "PK_audiofiles" PRIMARY KEY (id);


--
-- Name: auditlogrecords PK_auditlogrecords; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY auditlogrecords
    ADD CONSTRAINT "PK_auditlogrecords" PRIMARY KEY (id);


--
-- Name: brokeninterviewpackages PK_brokeninterviewpackages; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY brokeninterviewpackages
    ADD CONSTRAINT "PK_brokeninterviewpackages" PRIMARY KEY (id);


--
-- Name: completedemailrecords PK_completedemailrecords; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY completedemailrecords
    ADD CONSTRAINT "PK_completedemailrecords" PRIMARY KEY (interviewid);


--
-- Name: cumulativereportstatuschanges PK_cumulativereportstatuschanges; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY cumulativereportstatuschanges
    ADD CONSTRAINT "PK_cumulativereportstatuschanges" PRIMARY KEY (entryid);


--
-- Name: deviceinfos PK_deviceinfos; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY deviceinfos
    ADD CONSTRAINT "PK_deviceinfos" PRIMARY KEY (id);


--
-- Name: devicesyncinfo PK_devicesyncinfo; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY devicesyncinfo
    ADD CONSTRAINT "PK_devicesyncinfo" PRIMARY KEY ("Id");


--
-- Name: devicesyncstatistics PK_devicesyncstatistics; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY devicesyncstatistics
    ADD CONSTRAINT "PK_devicesyncstatistics" PRIMARY KEY ("Id");


--
-- Name: featuredquestions PK_featuredquestions; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY featuredquestions
    ADD CONSTRAINT "PK_featuredquestions" PRIMARY KEY (questionnaireid, "position");


--
-- Name: interview_geo_answers PK_interview_geo_answers_id; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY interview_geo_answers
    ADD CONSTRAINT "PK_interview_geo_answers_id" PRIMARY KEY (id);


--
-- Name: interviewpackages PK_interviewpackages; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY interviewpackages
    ADD CONSTRAINT "PK_interviewpackages" PRIMARY KEY (id);


--
-- Name: invitations PK_invitations; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY invitations
    ADD CONSTRAINT "PK_invitations" PRIMARY KEY (id);


--
-- Name: mapbrowseitems PK_mapbrowseitems; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY mapbrowseitems
    ADD CONSTRAINT "PK_mapbrowseitems" PRIMARY KEY (id);

--
-- Name: questionnairebrowseitems PK_questionnairebrowseitems; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY questionnairebrowseitems
    ADD CONSTRAINT "PK_questionnairebrowseitems" PRIMARY KEY (id);


--
-- Name: receivedpackagelogentries PK_receivedpackagelogentries; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY receivedpackagelogentries
    ADD CONSTRAINT "PK_receivedpackagelogentries" PRIMARY KEY (id);


--
-- Name: report_statistics PK_report_statistics; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY report_statistics
    ADD CONSTRAINT "PK_report_statistics" PRIMARY KEY (id);


--
-- Name: reusablecategoricaloptions PK_reusablecategoricaloptions; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY reusablecategoricaloptions
    ADD CONSTRAINT "PK_reusablecategoricaloptions" PRIMARY KEY (id);


--
-- Name: synchronizationlog PK_synchronizationlog; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY synchronizationlog
    ADD CONSTRAINT "PK_synchronizationlog" PRIMARY KEY (id);


--
-- Name: systemlog PK_systemlog; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY systemlog
    ADD CONSTRAINT "PK_systemlog" PRIMARY KEY (id);


--
-- Name: tablet_logs PK_tablet_logs; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY tablet_logs
    ADD CONSTRAINT "PK_tablet_logs" PRIMARY KEY (id);


--
-- Name: timespanbetweenstatuses PK_timespanbetweenstatuses; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY timespanbetweenstatuses
    ADD CONSTRAINT "PK_timespanbetweenstatuses" PRIMARY KEY (id);


--
-- Name: translationinstances PK_translationinstances; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY translationinstances
    ADD CONSTRAINT "PK_translationinstances" PRIMARY KEY (id);


--
-- Name: usermaps PK_usermaps; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY usermaps
    ADD CONSTRAINT "PK_usermaps" PRIMARY KEY (id);


--
-- Name: usersimportprocess PK_usersimportprocess; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY usersimportprocess
    ADD CONSTRAINT "PK_usersimportprocess" PRIMARY KEY (id);


--
-- Name: usertoimport PK_usertoimport; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY usertoimport
    ADD CONSTRAINT "PK_usertoimport" PRIMARY KEY (id);


--
-- Name: interview_geo_answers UC_interview_geo_answers_interviewid_questionid_rostervector; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY interview_geo_answers
    ADD CONSTRAINT "UC_interview_geo_answers_interviewid_questionid_rostervector" UNIQUE (interview_id, questionid, rostervector);


--
-- Name: appsettings appsettings_pkey; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY appsettings
    ADD CONSTRAINT appsettings_pkey PRIMARY KEY (id);


--
-- Name: events events_pk; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY events
    ADD CONSTRAINT events_pk PRIMARY KEY (globalsequence);


--
-- Name: interviewsummaries interviewsummaries_pk; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY interviewsummaries
    ADD CONSTRAINT interviewsummaries_pk PRIMARY KEY (id);


--
-- Name: interviewcommentedstatuses pk_interviewcommentedstatuses; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY interviewcommentedstatuses
    ADD CONSTRAINT pk_interviewcommentedstatuses PRIMARY KEY (id);


--
-- Name: interviewflags pk_interviewflags; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY interviewflags
    ADD CONSTRAINT pk_interviewflags PRIMARY KEY (interviewid, questionidentity);


--
-- Name: questionnaire_entities pk_questionnaire_entities; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY questionnaire_entities
    ADD CONSTRAINT pk_questionnaire_entities PRIMARY KEY (id);


--
-- Name: questionnaire_entities questionnaire_entities_un; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY questionnaire_entities
    ADD CONSTRAINT questionnaire_entities_un UNIQUE (questionnaireidentity, entityid);


--
-- Name: questionnairebackups questionnairebackups_pkey; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY questionnairebackups
    ADD CONSTRAINT questionnairebackups_pkey PRIMARY KEY (id);


--
-- Name: questionnairedocuments questionnairedocuments_pkey; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY questionnairedocuments
    ADD CONSTRAINT questionnairedocuments_pkey PRIMARY KEY (id);


--
-- Name: questionnairelookuptables questionnairelookuptables_pkey; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY questionnairelookuptables
    ADD CONSTRAINT questionnairelookuptables_pkey PRIMARY KEY (id);


--
-- Name: questionnairepdfs questionnairepdfs_pkey; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY questionnairepdfs
    ADD CONSTRAINT questionnairepdfs_pkey PRIMARY KEY (id);


--
-- Name: interviewsummaries unq_interviewsummaries_summaryid; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY interviewsummaries
    ADD CONSTRAINT unq_interviewsummaries_summaryid UNIQUE (summaryid);


--
-- Name: webinterviewconfigs webinterviewconfigs_pkey; Type: CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY webinterviewconfigs
    ADD CONSTRAINT webinterviewconfigs_pkey PRIMARY KEY (id);


--
-- Name: IX_assignments_id; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE UNIQUE INDEX "IX_assignments_id" ON assignments USING btree (id);


--
-- Name: IX_assignments_publickey; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX "IX_assignments_publickey" ON assignments USING btree (publickey);


--
-- Name: IX_assignments_responsibleid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX "IX_assignments_responsibleid" ON assignments USING btree (responsibleid);


--
-- Name: IX_commentaries_id; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX "IX_commentaries_id" ON commentaries USING btree (id);


--
-- Name: IX_commentaries_summary_id; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX "IX_commentaries_summary_id" ON commentaries USING btree (summary_id);


--
-- Name: IX_interviewsummaries_firstanswerdate; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX "IX_interviewsummaries_firstanswerdate" ON interviewsummaries USING btree (firstanswerdate);


--
-- Name: IX_interviewsummaries_questionnaire_variable; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX "IX_interviewsummaries_questionnaire_variable" ON interviewsummaries USING btree (questionnaire_variable);


--
-- Name: IX_receivedpackagelogentries_firsteventid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX "IX_receivedpackagelogentries_firsteventid" ON receivedpackagelogentries USING btree (firsteventid);


--
-- Name: answerstofeaturedquestions_answer_code_idx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX answerstofeaturedquestions_answer_code_idx ON identifyingentityvalue USING btree (answer_code);


--
-- Name: answerstofeaturedquestions_answer_lower_case_idx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX answerstofeaturedquestions_answer_lower_case_idx ON identifyingentityvalue USING btree (value_lower_case);


--
-- Name: answerstofeaturedquestions_answervalue; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX answerstofeaturedquestions_answervalue ON identifyingentityvalue USING btree (value text_pattern_ops);


--
-- Name: answerstofeaturedquestions_interview_id_position_idx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX answerstofeaturedquestions_interview_id_position_idx ON identifyingentityvalue USING btree (interview_id, "position");


--
-- Name: assignmentsidentifyinganswers_assignments; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX assignmentsidentifyinganswers_assignments ON assignmentsidentifyinganswers USING btree (assignmentid);


--
-- Name: audioauditfiles_interviewid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX audioauditfiles_interviewid ON audioauditfiles USING btree (interviewid);


--
-- Name: audiofiles_interviewid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX audiofiles_interviewid ON audiofiles USING btree (interviewid);


--
-- Name: auditlogrecords_recordid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX auditlogrecords_recordid ON auditlogrecords USING btree (recordid);


--
-- Name: auditlogrecords_responsibleid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX auditlogrecords_responsibleid ON auditlogrecords USING btree (responsibleid);


--
-- Name: commentaries_variable_idx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX commentaries_variable_idx ON commentaries USING btree (variable text_pattern_ops);


--
-- Name: cumulativereportstatuschanges_interviewid_idx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX cumulativereportstatuschanges_interviewid_idx ON cumulativereportstatuschanges USING btree (interviewid, changevalue, eventsequence DESC);


--
-- Name: event_source_eventsequence_indx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE UNIQUE INDEX event_source_eventsequence_indx ON events USING btree (eventsourceid, eventsequence);


--
-- Name: idx_Questionnaire_id; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX "idx_Questionnaire_id" ON questionnairebrowseitems USING btree (questionnaireid);


--
-- Name: idx_categories_reusablecategoricaloptions; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX idx_categories_reusablecategoricaloptions ON reusablecategoricaloptions USING btree (categoriesid, questionnaireid, questionnaireversion);


--
-- Name: idx_report_statistics_entityid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX idx_report_statistics_entityid ON report_statistics USING btree (entity_id);


--
-- Name: idx_report_statistics_interviewid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX idx_report_statistics_interviewid ON report_statistics USING btree (interview_id);


--
-- Name: idx_sortindex_reusablecategoricaloptions; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX idx_sortindex_reusablecategoricaloptions ON reusablecategoricaloptions USING btree (sortindex);


--
-- Name: interviewcommentedstatuses_interview_id_idx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewcommentedstatuses_interview_id_idx ON interviewcommentedstatuses USING btree (interview_id, "position");


--
-- Name: interviewpackage_interviewid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewpackage_interviewid ON interviewpackages USING btree (interviewid);


--
-- Name: interviewstatustimespans_timespansbetweenstatuses; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewstatustimespans_timespansbetweenstatuses ON timespanbetweenstatuses USING btree (interviewstatustimespans);


--
-- Name: interviewsummaries_assignmentid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewsummaries_assignmentid ON interviewsummaries USING btree (assignmentid);


--
-- Name: interviewsummaries_interviewid_unique_idx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE UNIQUE INDEX interviewsummaries_interviewid_unique_idx ON interviewsummaries USING btree (interviewid);


--
-- Name: interviewsummaries_questionnaire_id_indx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewsummaries_questionnaire_id_indx ON interviewsummaries USING btree (questionnaireid);


--
-- Name: interviewsummaries_questionnaire_identity_indx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewsummaries_questionnaire_identity_indx ON interviewsummaries USING btree (questionnaireidentity);


--
-- Name: interviewsummaries_questionnaire_version_indx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewsummaries_questionnaire_version_indx ON interviewsummaries USING btree (questionnaireversion);


--
-- Name: interviewsummaries_responsible_name_lower_case_idx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewsummaries_responsible_name_lower_case_idx ON interviewsummaries USING btree (responsible_name_lower_case);


--
-- Name: interviewsummaries_responsibleid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewsummaries_responsibleid ON interviewsummaries USING btree (responsibleid);


--
-- Name: interviewsummaries_responsiblename; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewsummaries_responsiblename ON interviewsummaries USING btree (responsiblename);


--
-- Name: interviewsummaries_status; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewsummaries_status ON interviewsummaries USING btree (status);


--
-- Name: interviewsummaries_teamlead_name_lower_case_idx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewsummaries_teamlead_name_lower_case_idx ON interviewsummaries USING btree (teamlead_name_lower_case);


--
-- Name: interviewsummaries_teamleadid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewsummaries_teamleadid ON interviewsummaries USING btree (teamleadid);


--
-- Name: interviewsummaries_teamleadname; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX interviewsummaries_teamleadname ON interviewsummaries USING btree (teamleadname);


--
-- Name: interviewsummaries_unique_key; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE UNIQUE INDEX interviewsummaries_unique_key ON interviewsummaries USING btree (key, clientkey);


--
-- Name: questionnaire_entities_entityid_idx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX questionnaire_entities_entityid_idx ON questionnaire_entities USING btree (entityid);


--
-- Name: synchronizationlog_interviewerid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX synchronizationlog_interviewerid ON synchronizationlog USING btree (interviewerid);


--
-- Name: synchronizationlog_interviewid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX synchronizationlog_interviewid ON synchronizationlog USING btree (interviewid);


--
-- Name: systemlog_userid; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX systemlog_userid ON systemlog USING btree (userid);


--
-- Name: timespanbetweenstatuses_interviewid_idx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX timespanbetweenstatuses_interviewid_idx ON timespanbetweenstatuses USING btree (interview_id);


--
-- Name: translationinstances_questionnaire_indx; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX translationinstances_questionnaire_indx ON translationinstances USING btree (questionnaireid, questionnaireversion, translationid);


--
-- Name: userdocuments_deviceinfos; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX userdocuments_deviceinfos ON deviceinfos USING btree ("User");


--
-- Name: usermaps_map; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX usermaps_map ON usermaps USING btree (map);


--
-- Name: usermaps_user; Type: INDEX; Schema: ws_primary; Owner: -
--

CREATE INDEX usermaps_user ON usermaps USING btree (username);


--
-- Name: identifyingentityvalue FK_answerstofeaturedquestions_interview_id_interviewsummaries_i; Type: FK CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY identifyingentityvalue
    ADD CONSTRAINT "FK_answerstofeaturedquestions_interview_id_interviewsummaries_i" FOREIGN KEY (interview_id) REFERENCES interviewsummaries(id) ON DELETE CASCADE;


--
-- Name: identifyingentityvalue FK_answerstofeaturedquestions_question_id_questionnaire_entitie; Type: FK CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY identifyingentityvalue
    ADD CONSTRAINT "FK_answerstofeaturedquestions_question_id_questionnaire_entitie" FOREIGN KEY (entity_id) REFERENCES questionnaire_entities(id);


--
-- Name: devicesyncinfo FK_devicesyncinfo_StatisticsId_devicesyncstatistics_Id; Type: FK CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY devicesyncinfo
    ADD CONSTRAINT "FK_devicesyncinfo_StatisticsId_devicesyncstatistics_Id" FOREIGN KEY ("StatisticsId") REFERENCES devicesyncstatistics("Id");


--
-- Name: assignmentsidentifyinganswers assignments_assignmentsidentifyinganswers; Type: FK CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY assignmentsidentifyinganswers
    ADD CONSTRAINT assignments_assignmentsidentifyinganswers FOREIGN KEY (assignmentid) REFERENCES assignments(id) ON DELETE CASCADE;


--
-- Name: commentaries fk_commentaries_to_interviewsummaries; Type: FK CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY commentaries
    ADD CONSTRAINT fk_commentaries_to_interviewsummaries FOREIGN KEY (summary_id) REFERENCES interviewsummaries(id) ON DELETE CASCADE;


--
-- Name: cumulativereportstatuschanges fk_cumulativereportstatuschanges_to_interviewsummaries; Type: FK CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY cumulativereportstatuschanges
    ADD CONSTRAINT fk_cumulativereportstatuschanges_to_interviewsummaries FOREIGN KEY (interviewid) REFERENCES interviewsummaries(interviewid) ON DELETE CASCADE;


--
-- Name: interview_geo_answers fk_interview_geo_answers_to_interviewsummaries; Type: FK CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY interview_geo_answers
    ADD CONSTRAINT fk_interview_geo_answers_to_interviewsummaries FOREIGN KEY (interview_id) REFERENCES interviewsummaries(id) ON DELETE CASCADE;


--
-- Name: interviewflags fk_interviewsummary_interviewflag; Type: FK CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY interviewflags
    ADD CONSTRAINT fk_interviewsummary_interviewflag FOREIGN KEY (interviewid) REFERENCES interviewsummaries(summaryid) ON DELETE CASCADE;


--
-- Name: featuredquestions fk_questionnairebrowseitems_featuredquestions; Type: FK CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY featuredquestions
    ADD CONSTRAINT fk_questionnairebrowseitems_featuredquestions FOREIGN KEY (questionnaireid) REFERENCES questionnairebrowseitems(id);


--
-- Name: report_statistics fk_report_statistics_to_interviewsummaries; Type: FK CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY report_statistics
    ADD CONSTRAINT fk_report_statistics_to_interviewsummaries FOREIGN KEY (interview_id) REFERENCES interviewsummaries(id) ON DELETE CASCADE;


--
-- Name: usermaps mapbrowseitems_mapbrowseitems; Type: FK CONSTRAINT; Schema: ws_primary; Owner: -
--

ALTER TABLE ONLY usermaps
    ADD CONSTRAINT mapbrowseitems_mapbrowseitems FOREIGN KEY (map) REFERENCES mapbrowseitems(id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

CREATE OR REPLACE FUNCTION _final_median(numeric[])
   RETURNS numeric AS
$$
   SELECT AVG(val)
   FROM (
     SELECT val
     FROM unnest($1) val
     ORDER BY 1
     LIMIT  2 - MOD(array_upper($1, 1), 2)
     OFFSET CEIL(array_upper($1, 1) / 2.0) - 1
   ) sub;
$$
LANGUAGE 'sql' IMMUTABLE;

CREATE AGGREGATE median(numeric) (
  SFUNC=array_append,
  STYPE=numeric[],
  FINALFUNC=_final_median,
  INITCOND='{}'
);
