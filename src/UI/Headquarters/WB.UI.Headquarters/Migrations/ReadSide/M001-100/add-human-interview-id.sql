-- https://wiki.postgresql.org/wiki/Pseudo_encrypt
CREATE OR REPLACE FUNCTION readside.pseudo_encrypt(VALUE bigint) returns int AS $$
DECLARE
l1 int;
l2 int;
r1 int;
r2 int;
i int:= 0;
BEGIN
l1:= (VALUE >> 16) & 65535;
r1:= VALUE & 65535;
WHILE i < 3 LOOP
l2 := r1;
r2:= l1 # ((((1366 * r1 + 150889) % 714025) / 714025.0) * 32767)::int;
l1:= l2;
r1:= r2;
i:= i + 1;
END LOOP;
RETURN((r1 << 16) + l1);
END;
$$ LANGUAGE plpgsql strict immutable;


CREATE SEQUENCE readside.interview_humanid_seq INCREMENT BY 1;

CREATE FUNCTION readside.trigger_interviewsummaries_before_insert () RETURNS trigger AS $$
BEGIN
NEW.humanid = readside.pseudo_encrypt(nextval('readside.interview_humanid_seq'));
return NEW;
END;
$$ LANGUAGE  plpgsql;


CREATE TRIGGER interviewsummaries_before_insert
BEFORE INSERT ON readside.interviewsummaries FOR EACH ROW 
EXECUTE PROCEDURE readside.trigger_interviewsummaries_before_insert ();

UPDATE readside.interviewsummaries SET humanId = readside.pseudo_encrypt(nextval('readside.interview_humanid_seq'));