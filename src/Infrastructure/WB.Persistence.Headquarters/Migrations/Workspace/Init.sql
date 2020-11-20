-- noinspection SqlNoDataSourceInspectionForFile

CREATE SEQUENCE "__interview_geo_answers_id_seq"
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE assignment_id_sequence;

CREATE SEQUENCE assignment_id_sequence
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE assignmentsimportprocess_id_seq;

CREATE SEQUENCE assignmentsimportprocess_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE assignmenttoimport_id_seq;

CREATE SEQUENCE assignmenttoimport_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE auditlogrecords_id_seq;

CREATE SEQUENCE auditlogrecords_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE brokeninterviewpackages_id_seq;

CREATE SEQUENCE brokeninterviewpackages_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE commentaries_id_seq;

CREATE SEQUENCE commentaries_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE "devicesyncinfo_Id_seq";

CREATE SEQUENCE "devicesyncinfo_Id_seq"
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE "devicesyncstatistics_Id_seq";

CREATE SEQUENCE "devicesyncstatistics_Id_seq"
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE globalsequence;

CREATE SEQUENCE globalsequence
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE interviewpackages_id_seq;

CREATE SEQUENCE interviewpackages_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE interviewsummaries_id_seq;

CREATE SEQUENCE interviewsummaries_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE invitations_id_seq;

CREATE SEQUENCE invitations_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE questionnaire_entities_id_seq;

CREATE SEQUENCE questionnaire_entities_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE receivedpackagelogentries_id_seq;

CREATE SEQUENCE receivedpackagelogentries_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE report_statistics_id_seq;

CREATE SEQUENCE report_statistics_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE reusablecategoricaloptions_id_seq;

CREATE SEQUENCE reusablecategoricaloptions_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE systemlog_id_seq;

CREATE SEQUENCE systemlog_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE tablet_logs_id_seq;

CREATE SEQUENCE tablet_logs_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE usersimportprocess_id_seq;

CREATE SEQUENCE usersimportprocess_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;
-- DROP SEQUENCE usertoimport_id_seq;

CREATE SEQUENCE usertoimport_id_seq
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START 1
    CACHE 1
    NO CYCLE;-- "VersionInfo" definition

-- Drop table

-- DROP TABLE "VersionInfo";

CREATE TABLE appsettings (
                             id text NOT NULL,
                             value jsonb NOT NULL,
                             CONSTRAINT appsettings_pkey PRIMARY KEY (id)
);


-- assemblyinfos definition

-- Drop table

-- DROP TABLE assemblyinfos;

CREATE TABLE assemblyinfos (
                               id varchar(255) NOT NULL,
                               creationdate timestamp NULL,
                               "content" bytea NULL,
                               CONSTRAINT "PK_assemblyinfos" PRIMARY KEY (id)
);

CREATE TABLE assignments (
                             publickey uuid NOT NULL,
                             id int4 NOT NULL,
                             responsibleid uuid NOT NULL,
                             quantity int4 NULL,
                             archived bool NOT NULL,
                             createdatutc timestamp NOT NULL,
                             updatedatutc timestamp NOT NULL,
                             questionnaireid uuid NOT NULL,
                             questionnaireversion int4 NOT NULL,
                             questionnaire varchar(255) NULL,
                             answers jsonb NULL,
                             protectedvariables jsonb NULL,
                             receivedbytabletatutc timestamp NULL,
                             audiorecording bool NOT NULL DEFAULT false,
                             email text NULL,
                             "password" text NULL,
                             webmode bool NULL,
                             "comments" text NULL,
                             CONSTRAINT "PK_assignments" PRIMARY KEY (publickey)
);
CREATE UNIQUE INDEX "IX_assignments_id" ON assignments USING btree (id);
CREATE INDEX "IX_assignments_publickey" ON assignments USING btree (publickey);
CREATE INDEX "IX_assignments_responsibleid" ON assignments USING btree (responsibleid);


-- assignmentsimportprocess definition

-- Drop table

-- DROP TABLE assignmentsimportprocess;

CREATE TABLE assignmentsimportprocess (
                                          id serial NOT NULL DEFAULT nextval('assignmentsimportprocess_id_seq'::regclass),
                                          questionnaireid text NOT NULL,
                                          filename text NOT NULL,
                                          totalcount int4 NOT NULL,
                                          responsible text NOT NULL,
                                          starteddate timestamp NOT NULL,
                                          status int4 NULL,
                                          assignedto uuid NOT NULL,
                                          CONSTRAINT "PK_assignmentsimportprocess" PRIMARY KEY (id)
);


-- assignmenttoimport definition

-- Drop table

-- DROP TABLE assignmenttoimport;

CREATE TABLE assignmenttoimport (
                                    id serial NOT NULL DEFAULT nextval('assignmenttoimport_id_seq'::regclass),
                                    interviewer uuid NULL,
                                    supervisor uuid NULL,
                                    quantity int4 NULL,
                                    answers jsonb NULL,
                                    verified bool NOT NULL DEFAULT false,
                                    error text NULL,
                                    protectedvariables jsonb NULL,
                                    email text NULL,
                                    "password" text NULL,
                                    webmode bool NULL,
                                    isaudiorecordingenabled bool NULL,
                                    headquarters uuid NULL,
                                    "comments" text NULL,
                                    CONSTRAINT "PK_assignmenttoimport" PRIMARY KEY (id)
);


-- attachmentcontents definition

-- Drop table

-- DROP TABLE attachmentcontents;

CREATE TABLE attachmentcontents (
                                    id varchar(255) NOT NULL,
                                    contenttype text NULL,
                                    "content" bytea NULL,
                                    filename text NULL,
                                    CONSTRAINT "PK_attachmentcontents" PRIMARY KEY (id)
);


-- audioauditfiles definition

-- Drop table

-- DROP TABLE audioauditfiles;

CREATE TABLE audioauditfiles (
                                 id text NOT NULL,
                                 interviewid uuid NOT NULL,
                                 filename text NOT NULL,
                                 contenttype text NULL,
                                 "data" bytea NULL,
                                 CONSTRAINT "PK_audioauditfiles" PRIMARY KEY (id)
);
CREATE INDEX audioauditfiles_interviewid ON audioauditfiles USING btree (interviewid);


-- audiofiles definition

-- Drop table

-- DROP TABLE audiofiles;

CREATE TABLE audiofiles (
                            id text NOT NULL,
                            interviewid uuid NOT NULL,
                            filename text NOT NULL,
                            "data" bytea NOT NULL,
                            contenttype text NULL,
                            CONSTRAINT "PK_audiofiles" PRIMARY KEY (id)
);
CREATE INDEX audiofiles_interviewid ON audiofiles USING btree (interviewid);


-- auditlogrecords definition

-- Drop table

-- DROP TABLE auditlogrecords;

CREATE TABLE auditlogrecords (
                                 id serial NOT NULL DEFAULT nextval('auditlogrecords_id_seq'::regclass),
                                 recordid int4 NOT NULL,
                                 responsibleid uuid NULL,
                                 responsiblename text NULL,
                                 "type" text NOT NULL,
                                 "time" timestamp NOT NULL,
                                 timeutc timestamp NOT NULL,
                                 payload text NOT NULL,
                                 CONSTRAINT "PK_auditlogrecords" PRIMARY KEY (id)
);
CREATE INDEX auditlogrecords_recordid ON auditlogrecords USING btree (recordid);
CREATE INDEX auditlogrecords_responsibleid ON auditlogrecords USING btree (responsibleid);


-- brokeninterviewpackages definition

-- Drop table

-- DROP TABLE brokeninterviewpackages;

CREATE TABLE brokeninterviewpackages (
                                         id serial NOT NULL DEFAULT nextval('brokeninterviewpackages_id_seq'::regclass),
                                         interviewid uuid NULL,
                                         questionnaireid uuid NULL,
                                         questionnaireversion int8 NULL,
                                         responsibleid uuid NULL,
                                         interviewstatus int4 NULL,
                                         iscensusinterview bool NULL,
                                         incomingdate timestamp NULL,
                                         events text NULL,
                                         processingdate timestamp NULL,
                                         exceptiontype text NULL,
                                         exceptionmessage text NULL,
                                         exceptionstacktrace text NULL,
                                         packagesize int8 NULL,
                                         reprocessattemptscount int4 NOT NULL,
                                         interviewkey varchar(12) NULL,
                                         CONSTRAINT "PK_brokeninterviewpackages" PRIMARY KEY (id)
);


-- completedemailrecords definition

-- Drop table

-- DROP TABLE completedemailrecords;

CREATE TABLE completedemailrecords (
                                       interviewid uuid NOT NULL,
                                       requesttime timestamp NOT NULL,
                                       failedcount int4 NOT NULL,
                                       CONSTRAINT "PK_completedemailrecords" PRIMARY KEY (interviewid)
);


-- deviceinfos definition

-- Drop table

-- DROP TABLE deviceinfos;

CREATE TABLE deviceinfos (
                             id int4 NOT NULL,
                             "User" varchar(255) NULL,
                             "date" timestamp NULL,
                             deviceid text NULL,
                             userid varchar(255) NULL,
                             CONSTRAINT "PK_deviceinfos" PRIMARY KEY (id)
);
CREATE INDEX userdocuments_deviceinfos ON deviceinfos USING btree ("User");


-- devicesyncstatistics definition

-- Drop table

-- DROP TABLE devicesyncstatistics;

CREATE TABLE devicesyncstatistics (
                                      "Id" serial NOT NULL DEFAULT nextval('"devicesyncstatistics_Id_seq"'::regclass),
                                      "UploadedInterviewsCount" int4 NOT NULL,
                                      "DownloadedInterviewsCount" int4 NOT NULL,
                                      "DownloadedQuestionnairesCount" int4 NOT NULL,
                                      "RejectedInterviewsOnDeviceCount" int4 NOT NULL,
                                      "NewInterviewsOnDeviceCount" int4 NOT NULL,
                                      "NewAssignmentsCount" int4 NOT NULL,
                                      "RemovedAssignmentsCount" int4 NOT NULL,
                                      "RemovedInterviewsCount" int4 NULL,
                                      "AssignmentsOnDeviceCount" int4 NOT NULL,
                                      "TotalUploadedBytes" int8 NOT NULL,
                                      "TotalDownloadedBytes" int8 NOT NULL,
                                      "TotalConnectionSpeed" float8 NOT NULL,
                                      "TotalSyncDuration" interval(6) NOT NULL,
                                      "SyncFinishDate" timestamp NOT NULL,
                                      CONSTRAINT "PK_devicesyncstatistics" PRIMARY KEY ("Id")
);


-- events definition

-- Drop table

-- DROP TABLE events;

CREATE TABLE events (
                        id uuid NOT NULL,
                        origin text NULL,
                        "timestamp" timestamp NOT NULL,
                        eventsourceid uuid NOT NULL,
                        globalsequence int4 NOT NULL DEFAULT nextval('globalsequence'::regclass),
                        value jsonb NOT NULL,
                        eventsequence int4 NOT NULL,
                        eventtype text NOT NULL,
                        CONSTRAINT events_pk PRIMARY KEY (globalsequence)
);
CREATE UNIQUE INDEX event_source_eventsequence_indx ON events USING btree (eventsourceid, eventsequence);


-- hibernate_unique_key definition

-- Drop table

-- DROP TABLE hibernate_unique_key;

CREATE TABLE hibernate_unique_key (
    next_hi int4 NOT NULL
);


-- interviewcommentedstatuses definition

-- Drop table

-- DROP TABLE interviewcommentedstatuses;

CREATE TABLE interviewcommentedstatuses (
                                            "position" int4 NOT NULL,
                                            id uuid NOT NULL,
                                            interviewername text NULL,
                                            interviewerid uuid NULL,
                                            supervisorid uuid NULL,
                                            supervisorname text NULL,
                                            statuschangeoriginatorid uuid NULL,
                                            statuschangeoriginatorname text NULL,
                                            statuschangeoriginatorrole int4 NULL,
                                            status int4 NULL,
                                            "timestamp" timestamp NULL,
                                            timespanwithpreviousstatus int8 NULL,
                                            "comment" text NULL,
                                            interview_id int4 NOT NULL,
                                            CONSTRAINT pk_interviewcommentedstatuses PRIMARY KEY (id)
);
CREATE INDEX interviewcommentedstatuses_interview_id_idx ON interviewcommentedstatuses USING btree (interview_id, "position");


-- interviewpackages definition

-- Drop table

-- DROP TABLE interviewpackages;

CREATE TABLE interviewpackages (
                                   id serial NOT NULL DEFAULT nextval('interviewpackages_id_seq'::regclass),
                                   interviewid uuid NULL,
                                   questionnaireid uuid NULL,
                                   questionnaireversion int8 NULL,
                                   responsibleid uuid NULL,
                                   interviewstatus int4 NULL,
                                   iscensusinterview bool NULL,
                                   incomingdate timestamp NULL,
                                   events text NULL,
                                   processattemptscount int4 NOT NULL,
                                   CONSTRAINT "PK_interviewpackages" PRIMARY KEY (id)
);
CREATE INDEX interviewpackage_interviewid ON interviewpackages USING btree (interviewid);


-- interviewsummaries definition

-- Drop table

-- DROP TABLE interviewsummaries;

CREATE TABLE interviewsummaries (
                                    summaryid varchar(255) NOT NULL,
                                    interviewid uuid NULL,
                                    questionnairetitle text NULL,
                                    responsiblename text NULL,
                                    teamleadid uuid NOT NULL,
                                    teamleadname text NULL,
                                    responsiblerole int4 NULL,
                                    updatedate timestamp NULL,
                                    wasrejectedbysupervisor bool NULL,
                                    wascreatedonclient bool NULL,
                                    questionnaireid uuid NULL,
                                    questionnaireversion int8 NULL,
                                    responsibleid uuid NULL,
                                    status int4 NULL,
                                    isassignedtointerviewer bool NOT NULL,
                                    "key" text NULL,
                                    assignmentid int4 NULL,
                                    clientkey varchar(12) NULL,
                                    questionnaireidentity text NULL,
                                    wascompleted bool NOT NULL DEFAULT false,
                                    interviewduration int8 NULL,
                                    lastresumeeventutctimestamp timestamp NULL,
                                    errorscount int4 NOT NULL DEFAULT 0,
                                    id serial NOT NULL DEFAULT nextval('interviewsummaries_id_seq'::regclass),
                                    createddate timestamp NULL,
                                    firstanswerdate timestamp NULL,
                                    firstinterviewerid uuid NULL,
                                    firstinterviewername text NULL,
                                    firstsupervisorid uuid NULL,
                                    firstsupervisorname text NULL,
                                    hasresolvedcomments bool NOT NULL DEFAULT false,
                                    responsible_name_lower_case text NULL,
                                    teamlead_name_lower_case text NULL,
                                    questionnaire_variable text NOT NULL,
                                    hassmallsubstitutions bool NOT NULL DEFAULT false,
                                    receivedbyintervieweratutc timestamp NULL,
                                    not_answered_count int4 NULL,
                                    CONSTRAINT interviewsummaries_pk PRIMARY KEY (id),
                                    CONSTRAINT unq_interviewsummaries_summaryid UNIQUE (summaryid)
);
CREATE INDEX "IX_interviewsummaries_firstanswerdate" ON interviewsummaries USING btree (firstanswerdate);
CREATE INDEX "IX_interviewsummaries_questionnaire_variable" ON interviewsummaries USING btree (questionnaire_variable);
CREATE INDEX interviewsummaries_assignmentid ON interviewsummaries USING btree (assignmentid);
CREATE UNIQUE INDEX interviewsummaries_interviewid_unique_idx ON interviewsummaries USING btree (interviewid);
CREATE INDEX interviewsummaries_questionnaire_id_indx ON interviewsummaries USING btree (questionnaireid);
CREATE INDEX interviewsummaries_questionnaire_identity_indx ON interviewsummaries USING btree (questionnaireidentity);
CREATE INDEX interviewsummaries_questionnaire_version_indx ON interviewsummaries USING btree (questionnaireversion);
CREATE INDEX interviewsummaries_responsible_name_lower_case_idx ON interviewsummaries USING btree (responsible_name_lower_case);
CREATE INDEX interviewsummaries_responsibleid ON interviewsummaries USING btree (responsibleid);
CREATE INDEX interviewsummaries_responsiblename ON interviewsummaries USING btree (responsiblename);
CREATE INDEX interviewsummaries_status ON interviewsummaries USING btree (status);
CREATE INDEX interviewsummaries_teamlead_name_lower_case_idx ON interviewsummaries USING btree (teamlead_name_lower_case);
CREATE INDEX interviewsummaries_teamleadid ON interviewsummaries USING btree (teamleadid);
CREATE INDEX interviewsummaries_teamleadname ON interviewsummaries USING btree (teamleadname);
CREATE UNIQUE INDEX interviewsummaries_unique_key ON interviewsummaries USING btree (key, clientkey);


-- invitations definition

-- Drop table

-- DROP TABLE invitations;

CREATE TABLE invitations (
                             id serial NOT NULL DEFAULT nextval('invitations_id_seq'::regclass),
                             assignmentid int4 NOT NULL,
                             interviewid text NULL,
                             "token" text NULL,
                             resumepassword text NULL,
                             sentonutc timestamp NULL,
                             invitationemailid text NULL,
                             lastremindersentonutc timestamp NULL,
                             lastreminderemailid text NULL,
                             numberofreminderssent text NULL,
                             last_rejected_interview_email_id text NULL,
                             last_rejected_status_position int4 NULL,
                             CONSTRAINT "PK_invitations" PRIMARY KEY (id)
);


-- mapbrowseitems definition

-- Drop table

-- DROP TABLE mapbrowseitems;

CREATE TABLE mapbrowseitems (
                                id varchar(255) NOT NULL,
                                "size" int8 NULL,
                                importdate timestamp NULL,
                                filename text NULL,
                                wkid int8 NULL,
                                xmaxval float8 NULL,
                                xminval float8 NULL,
                                ymaxval float8 NULL,
                                yminval float8 NULL,
                                maxscale float8 NULL,
                                minscale float8 NULL,
                                CONSTRAINT "PK_mapbrowseitems" PRIMARY KEY (id)
);


-- productversionhistory definition

-- Drop table

-- DROP TABLE productversionhistory;

CREATE TABLE productversionhistory (
                                       updatetimeutc timestamp NOT NULL,
                                       productversion text NULL,
                                       CONSTRAINT "PK_productversionhistory" PRIMARY KEY (updatetimeutc)
);


-- questionnaire_entities definition

-- Drop table

-- DROP TABLE questionnaire_entities;

CREATE TABLE questionnaire_entities (
                                        id serial NOT NULL DEFAULT nextval('questionnaire_entities_id_seq'::regclass),
                                        questionnaireidentity varchar(255) NOT NULL,
                                        entityid uuid NOT NULL,
                                        parentid uuid NULL,
                                        question_type int4 NULL,
                                        featured bool NULL,
                                        question_scope int4 NULL,
                                        entity_type int4 NULL,
                                        is_filtered_combobox bool NOT NULL DEFAULT false,
                                        linked_to_question_id uuid NULL,
                                        linked_to_roster_id uuid NULL,
                                        stata_export_caption text NULL,
                                        variable_label text NULL,
                                        question_text text NULL,
                                        cascade_from_question_id uuid NULL,
                                        CONSTRAINT pk_questionnaire_entities PRIMARY KEY (id),
                                        CONSTRAINT questionnaire_entities_un UNIQUE (questionnaireidentity, entityid)
);
CREATE INDEX questionnaire_entities_entityid_idx ON questionnaire_entities USING btree (entityid);


-- questionnairebackups definition

-- Drop table

-- DROP TABLE questionnairebackups;

CREATE TABLE questionnairebackups (
                                      id text NOT NULL,
                                      value json NOT NULL,
                                      CONSTRAINT questionnairebackups_pkey PRIMARY KEY (id)
);


-- questionnairebrowseitems definition

-- Drop table

-- DROP TABLE questionnairebrowseitems;

CREATE TABLE questionnairebrowseitems (
                                          id varchar(255) NOT NULL,
                                          creationdate timestamp NULL,
                                          questionnaireid uuid NULL,
                                          "version" int8 NULL,
                                          lastentrydate timestamp NULL,
                                          title text NULL,
                                          ispublic bool NULL,
                                          createdby uuid NULL,
                                          isdeleted bool NULL,
                                          allowcensusmode bool NULL,
                                          disabled bool NULL,
                                          questionnairecontentversion int8 NULL,
                                          importdate timestamp NULL,
                                          allowassignments bool NOT NULL,
                                          allowexportvariables bool NOT NULL,
                                          variable text NULL,
                                          disabledby uuid NULL,
                                          isaudiorecordingenabled bool NOT NULL DEFAULT false,
                                          "comment" text NULL,
                                          importedby uuid NULL,
                                          CONSTRAINT "PK_questionnairebrowseitems" PRIMARY KEY (id)
);
CREATE INDEX "idx_Questionnaire_id" ON questionnairebrowseitems USING btree (questionnaireid);


-- questionnairedocuments definition

-- Drop table

-- DROP TABLE questionnairedocuments;

CREATE TABLE questionnairedocuments (
                                        id text NOT NULL,
                                        value json NOT NULL,
                                        CONSTRAINT questionnairedocuments_pkey PRIMARY KEY (id)
);


-- questionnairelookuptables definition

-- Drop table

-- DROP TABLE questionnairelookuptables;

CREATE TABLE questionnairelookuptables (
                                           id text NOT NULL,
                                           value json NOT NULL,
                                           CONSTRAINT questionnairelookuptables_pkey PRIMARY KEY (id)
);


-- questionnairepdfs definition

-- Drop table

-- DROP TABLE questionnairepdfs;

CREATE TABLE questionnairepdfs (
                                   id text NOT NULL,
                                   value json NOT NULL,
                                   CONSTRAINT questionnairepdfs_pkey PRIMARY KEY (id)
);


-- receivedpackagelogentries definition

-- Drop table

-- DROP TABLE receivedpackagelogentries;

CREATE TABLE receivedpackagelogentries (
                                           id serial NOT NULL DEFAULT nextval('receivedpackagelogentries_id_seq'::regclass),
                                           firsteventid uuid NOT NULL,
                                           lasteventid uuid NOT NULL,
                                           firsteventtimestamp timestamp NOT NULL,
                                           lasteventtimestamp timestamp NOT NULL,
                                           CONSTRAINT "PK_receivedpackagelogentries" PRIMARY KEY (id)
);
CREATE INDEX "IX_receivedpackagelogentries_firsteventid" ON receivedpackagelogentries USING btree (firsteventid);


-- reusablecategoricaloptions definition

-- Drop table

-- DROP TABLE reusablecategoricaloptions;

CREATE TABLE reusablecategoricaloptions (
                                            id serial NOT NULL DEFAULT nextval('reusablecategoricaloptions_id_seq'::regclass),
                                            questionnaireid uuid NOT NULL,
                                            questionnaireversion int4 NOT NULL,
                                            categoriesid uuid NOT NULL,
                                            sortindex int4 NOT NULL,
                                            parentvalue int4 NULL,
                                            value int4 NOT NULL,
                                            "text" text NOT NULL,
                                            CONSTRAINT "PK_reusablecategoricaloptions" PRIMARY KEY (id)
);
CREATE INDEX idx_categories_reusablecategoricaloptions ON reusablecategoricaloptions USING btree (categoriesid, questionnaireid, questionnaireversion);
CREATE INDEX idx_sortindex_reusablecategoricaloptions ON reusablecategoricaloptions USING btree (sortindex);


-- synchronizationlog definition

-- Drop table

-- DROP TABLE synchronizationlog;

CREATE TABLE synchronizationlog (
                                    id int4 NOT NULL,
                                    interviewerid uuid NULL,
                                    interviewername text NULL,
                                    deviceid text NULL,
                                    logdate timestamp NULL,
                                    "type" int4 NULL,
                                    log text NULL,
                                    interviewid uuid NULL,
                                    actionexceptionmessage text NULL,
                                    actionexceptiontype text NULL,
                                    CONSTRAINT "PK_synchronizationlog" PRIMARY KEY (id)
);
CREATE INDEX synchronizationlog_interviewerid ON synchronizationlog USING btree (interviewerid);
CREATE INDEX synchronizationlog_interviewid ON synchronizationlog USING btree (interviewid);


-- systemlog definition

-- Drop table

-- DROP TABLE systemlog;

CREATE TABLE systemlog (
                           id serial NOT NULL DEFAULT nextval('systemlog_id_seq'::regclass),
                           "type" int4 NOT NULL,
                           logdate timestamp NOT NULL,
                           userid uuid NULL,
                           username text NULL,
                           log text NOT NULL,
                           CONSTRAINT "PK_systemlog" PRIMARY KEY (id)
);
CREATE INDEX systemlog_userid ON systemlog USING btree (userid);


-- tablet_logs definition

-- Drop table

-- DROP TABLE tablet_logs;

CREATE TABLE tablet_logs (
                             id serial NOT NULL DEFAULT nextval('tablet_logs_id_seq'::regclass),
                             device_id text NULL,
                             user_name text NULL,
                             "content" bytea NOT NULL,
                             receive_date_utc timestamp NOT NULL,
                             CONSTRAINT "PK_tablet_logs" PRIMARY KEY (id)
);


-- timespanbetweenstatuses definition

-- Drop table

-- DROP TABLE timespanbetweenstatuses;

CREATE TABLE timespanbetweenstatuses (
                                         id int4 NOT NULL,
                                         supervisorid uuid NULL,
                                         supervisorname text NULL,
                                         interviewerid uuid NULL,
                                         interviewername text NULL,
                                         beginstatus int4 NULL,
                                         endstatus int4 NULL,
                                         endstatustimestamp timestamp NULL,
                                         timespan int8 NULL,
                                         interviewstatustimespans varchar(255) NULL,
                                         interview_id int4 NOT NULL,
                                         CONSTRAINT "PK_timespanbetweenstatuses" PRIMARY KEY (id)
);
CREATE INDEX interviewstatustimespans_timespansbetweenstatuses ON timespanbetweenstatuses USING btree (interviewstatustimespans);
CREATE INDEX timespanbetweenstatuses_interviewid_idx ON timespanbetweenstatuses USING btree (interview_id);


-- translationinstances definition

-- Drop table

-- DROP TABLE translationinstances;

CREATE TABLE translationinstances (
                                      id int4 NOT NULL,
                                      questionnaireid uuid NOT NULL,
                                      "type" int4 NOT NULL,
                                      questionnaireversion int8 NOT NULL,
                                      questionnaireentityid uuid NOT NULL,
                                      translationindex text NULL,
                                      translationid uuid NOT NULL,
                                      value text NOT NULL,
                                      CONSTRAINT "PK_translationinstances" PRIMARY KEY (id)
);
CREATE INDEX translationinstances_questionnaire_indx ON translationinstances USING btree (questionnaireid, questionnaireversion, translationid);


-- usersimportprocess definition

-- Drop table

-- DROP TABLE usersimportprocess;

CREATE TABLE usersimportprocess (
                                    id serial NOT NULL DEFAULT nextval('usersimportprocess_id_seq'::regclass),
                                    filename text NOT NULL,
                                    supervisorscount int4 NOT NULL,
                                    interviewerscount int4 NOT NULL,
                                    responsible text NOT NULL,
                                    starteddate timestamp NOT NULL,
                                    CONSTRAINT "PK_usersimportprocess" PRIMARY KEY (id)
);


-- usertoimport definition

-- Drop table

-- DROP TABLE usertoimport;

CREATE TABLE usertoimport (
                              id serial NOT NULL DEFAULT nextval('usertoimport_id_seq'::regclass),
                              login text NOT NULL,
                              email text NULL,
                              fullname text NULL,
                              "password" text NOT NULL,
                              phonenumber text NULL,
                              "role" text NOT NULL,
                              supervisor text NULL,
                              CONSTRAINT "PK_usertoimport" PRIMARY KEY (id)
);


-- webinterviewconfigs definition

-- Drop table

-- DROP TABLE webinterviewconfigs;

CREATE TABLE webinterviewconfigs (
                                     id text NOT NULL,
                                     value json NOT NULL,
                                     CONSTRAINT webinterviewconfigs_pkey PRIMARY KEY (id)
);


-- assignmentsidentifyinganswers definition

-- Drop table

-- DROP TABLE assignmentsidentifyinganswers;

CREATE TABLE assignmentsidentifyinganswers (
                                               assignmentid int4 NOT NULL,
                                               "position" int4 NOT NULL,
                                               questionid uuid NOT NULL,
                                               answer text NULL,
                                               answerasstring text NULL,
                                               rostervector _int4 NOT NULL,
                                               CONSTRAINT assignments_assignmentsidentifyinganswers FOREIGN KEY (assignmentid) REFERENCES assignments(id) ON DELETE CASCADE
);
CREATE INDEX assignmentsidentifyinganswers_assignments ON assignmentsidentifyinganswers USING btree (assignmentid);


-- commentaries definition

-- Drop table

-- DROP TABLE commentaries;

CREATE TABLE commentaries (
                              commentsequence int4 NULL,
                              originatorname text NULL,
                              originatoruserid uuid NULL,
                              originatorrole int4 NULL,
                              "timestamp" timestamp NULL,
                              variable text NULL,
                              roster text NULL,
                              rostervector _numeric NULL,
                              "comment" text NULL,
                              summary_id int4 NOT NULL,
                              id serial NOT NULL DEFAULT nextval('commentaries_id_seq'::regclass),
                              CONSTRAINT fk_commentaries_to_interviewsummaries FOREIGN KEY (summary_id) REFERENCES interviewsummaries(id) ON DELETE CASCADE
);
CREATE INDEX "IX_commentaries_id" ON commentaries USING btree (id);
CREATE INDEX "IX_commentaries_summary_id" ON commentaries USING btree (summary_id);
CREATE INDEX commentaries_variable_idx ON commentaries USING btree (variable text_pattern_ops);


-- cumulativereportstatuschanges definition

-- Drop table

-- DROP TABLE cumulativereportstatuschanges;

CREATE TABLE cumulativereportstatuschanges (
                                               entryid varchar(255) NOT NULL,
                                               "date" timestamp NULL,
                                               status int4 NULL,
                                               changevalue int4 NULL,
                                               questionnaireidentity text NULL,
                                               interviewid uuid NULL,
                                               eventsequence int4 NULL,
                                               CONSTRAINT "PK_cumulativereportstatuschanges" PRIMARY KEY (entryid),
                                               CONSTRAINT fk_cumulativereportstatuschanges_to_interviewsummaries FOREIGN KEY (interviewid) REFERENCES interviewsummaries(interviewid) ON DELETE CASCADE
);
CREATE INDEX cumulativereportstatuschanges_interviewid_idx ON cumulativereportstatuschanges USING btree (interviewid, changevalue, eventsequence DESC);


-- devicesyncinfo definition

-- Drop table

-- DROP TABLE devicesyncinfo;

CREATE TABLE devicesyncinfo (
                                "Id" serial NOT NULL DEFAULT nextval('"devicesyncinfo_Id_seq"'::regclass),
                                "SyncDate" timestamp NOT NULL,
                                "InterviewerId" uuid NOT NULL,
                                "DeviceId" text NULL,
                                "DeviceModel" text NULL,
                                "DeviceType" text NULL,
                                "DeviceDate" timestamp NOT NULL,
                                "DeviceLocationLat" float8 NULL,
                                "DeviceLocationLong" float8 NULL,
                                "DeviceLanguage" text NULL,
                                "DeviceManufacturer" text NULL,
                                "DeviceBuildNumber" text NULL,
                                "DeviceSerialNumber" text NULL,
                                "AndroidVersion" text NULL,
                                "AndroidSdkVersion" int4 NOT NULL,
                                "AndroidSdkVersionName" text NULL,
                                "AppVersion" text NULL,
                                "AppBuildVersion" int4 NOT NULL,
                                "LastAppUpdatedDate" timestamp NOT NULL,
                                "NetworkType" text NULL,
                                "NetworkSubType" text NULL,
                                "MobileOperator" text NULL,
                                "AppOrientation" text NULL,
                                "BatteryPowerSource" text NULL,
                                "BatteryChargePercent" int4 NOT NULL,
                                "IsPowerInSaveMode" bool NOT NULL,
                                "MobileSignalStrength" int4 NOT NULL,
                                "StorageTotalInBytes" int8 NOT NULL,
                                "StorageFreeInBytes" int8 NOT NULL,
                                "RAMTotalInBytes" int8 NOT NULL,
                                "RAMFreeInBytes" int8 NOT NULL,
                                "DBSizeInfo" int8 NOT NULL,
                                "NumberOfStartedInterviews" int4 NOT NULL,
                                "StatisticsId" int4 NULL,
                                CONSTRAINT "PK_devicesyncinfo" PRIMARY KEY ("Id"),
                                CONSTRAINT "FK_devicesyncinfo_StatisticsId_devicesyncstatistics_Id" FOREIGN KEY ("StatisticsId") REFERENCES devicesyncstatistics("Id")
);


-- featuredquestions definition

-- Drop table

-- DROP TABLE featuredquestions;

CREATE TABLE featuredquestions (
                                   questionnaireid varchar(255) NOT NULL,
                                   "position" int4 NOT NULL,
                                   id uuid NULL,
                                   title text NULL,
                                   caption text NULL,
                                   CONSTRAINT "PK_featuredquestions" PRIMARY KEY (questionnaireid, "position"),
                                   CONSTRAINT fk_questionnairebrowseitems_featuredquestions FOREIGN KEY (questionnaireid) REFERENCES questionnairebrowseitems(id)
);


-- identifyingentityvalue definition

-- Drop table

-- DROP TABLE identifyingentityvalue;

CREATE TABLE identifyingentityvalue (
                                        id int4 NOT NULL,
                                        value text NULL,
                                        interview_id int4 NULL,
                                        "position" int4 NULL,
                                        entity_id int4 NULL,
                                        answer_code int4 NULL,
                                        value_lower_case text NULL,
                                        CONSTRAINT "PK_answerstofeaturedquestions" PRIMARY KEY (id),
                                        CONSTRAINT "FK_answerstofeaturedquestions_interview_id_interviewsummaries_i" FOREIGN KEY (interview_id) REFERENCES interviewsummaries(id) ON DELETE CASCADE,
                                        CONSTRAINT "FK_answerstofeaturedquestions_question_id_questionnaire_entitie" FOREIGN KEY (entity_id) REFERENCES questionnaire_entities(id)
);
CREATE INDEX answerstofeaturedquestions_answer_code_idx ON identifyingentityvalue USING btree (answer_code);
CREATE INDEX answerstofeaturedquestions_answer_lower_case_idx ON identifyingentityvalue USING btree (value_lower_case);
CREATE INDEX answerstofeaturedquestions_answervalue ON identifyingentityvalue USING btree (value text_pattern_ops);
CREATE INDEX answerstofeaturedquestions_interview_id_position_idx ON identifyingentityvalue USING btree (interview_id, "position");


-- interview_geo_answers definition

-- Drop table

-- DROP TABLE interview_geo_answers;

CREATE TABLE interview_geo_answers (
                                       id serial NOT NULL DEFAULT nextval('__interview_geo_answers_id_seq'::regclass),
                                       interview_id int4 NOT NULL,
                                       questionid uuid NOT NULL,
                                       rostervector text NOT NULL,
                                       latitude float8 NOT NULL,
                                       longitude float8 NOT NULL,
                                       "timestamp" text NULL,
                                       isenabled bool NOT NULL DEFAULT true,
                                       CONSTRAINT "PK_interview_geo_answers_id" PRIMARY KEY (id),
                                       CONSTRAINT "UC_interview_geo_answers_interviewid_questionid_rostervector" UNIQUE (interview_id, questionid, rostervector),
                                       CONSTRAINT fk_interview_geo_answers_to_interviewsummaries FOREIGN KEY (interview_id) REFERENCES interviewsummaries(id) ON DELETE CASCADE
);


-- interviewflags definition

-- Drop table

-- DROP TABLE interviewflags;

CREATE TABLE interviewflags (
                                interviewid bpchar(255) NOT NULL,
                                questionidentity text NOT NULL,
                                CONSTRAINT pk_interviewflags PRIMARY KEY (interviewid, questionidentity),
                                CONSTRAINT fk_interviewsummary_interviewflag FOREIGN KEY (interviewid) REFERENCES interviewsummaries(summaryid) ON DELETE CASCADE
);


-- report_statistics definition

-- Drop table

-- DROP TABLE report_statistics;

CREATE TABLE report_statistics (
                                   id serial NOT NULL DEFAULT nextval('report_statistics_id_seq'::regclass),
                                   interview_id int4 NOT NULL,
                                   entity_id int4 NOT NULL,
                                   rostervector text NOT NULL,
                                   answer _int8 NOT NULL,
                                   "type" int2 NOT NULL DEFAULT 0,
                                   is_enabled bool NOT NULL DEFAULT true,
                                   CONSTRAINT "PK_report_statistics" PRIMARY KEY (id),
                                   CONSTRAINT fk_report_statistics_to_interviewsummaries FOREIGN KEY (interview_id) REFERENCES interviewsummaries(id) ON DELETE CASCADE
);
CREATE INDEX idx_report_statistics_entityid ON report_statistics USING btree (entity_id);
CREATE INDEX idx_report_statistics_interviewid ON report_statistics USING btree (interview_id);


-- usermaps definition

-- Drop table

-- DROP TABLE usermaps;

CREATE TABLE usermaps (
                          id int4 NOT NULL,
                          "map" varchar(255) NOT NULL,
                          username varchar(255) NOT NULL,
                          CONSTRAINT "PK_usermaps" PRIMARY KEY (id),
                          CONSTRAINT mapbrowseitems_mapbrowseitems FOREIGN KEY (map) REFERENCES mapbrowseitems(id) ON DELETE CASCADE
);
CREATE INDEX usermaps_map ON usermaps USING btree (map);
CREATE INDEX usermaps_user ON usermaps USING btree (username);


-- report_statistics_categorical source

CREATE OR REPLACE VIEW report_statistics_categorical
AS SELECT a.interview_id,
          a.entity_id,
          a.rostervector,
          ans.ans AS answer
   FROM report_statistics a,
        LATERAL unnest(a.answer) ans(ans)
   WHERE a.type = 0 AND a.is_enabled = true;


-- report_statistics_numeric source

CREATE OR REPLACE VIEW report_statistics_numeric
AS SELECT a.interview_id,
          a.entity_id,
          a.rostervector,
          ans.ans AS answer
   FROM report_statistics a,
        LATERAL unnest(a.answer) ans(ans)
   WHERE a.type = 1 AND a.is_enabled = true;



CREATE OR REPLACE FUNCTION _final_median(numeric[])
    RETURNS numeric
    LANGUAGE sql
    IMMUTABLE
AS $function$
SELECT AVG(val)
FROM (
         SELECT val
         FROM unnest($1) val
         ORDER BY 1
         LIMIT  2 - MOD(array_upper($1, 1), 2)
             OFFSET CEIL(array_upper($1, 1) / 2.0) - 1
     ) sub;
$function$
;

aggregate_dummy;
