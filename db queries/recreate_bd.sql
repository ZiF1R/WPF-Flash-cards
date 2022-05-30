CREATE DATABASE FlashCards;
go
use FlashCards;

CREATE TABLE USERS (
	UNIQUE_ID int IDENTITY(1, 1) PRIMARY KEY,
	NICKNAME varchar(50),
	SURNAME varchar(50),
	NAME varchar(50),
	EMAIL varchar(50) UNIQUE NOT NULL,
	PASS varchar(MAX) NOT NULL,
);

CREATE TABLE CATEGORIES (
	CATEGORY_ID int IDENTITY(1, 1) PRIMARY KEY,
	USER_UID int CONSTRAINT CAT_USR_ID FOREIGN KEY REFERENCES USERS(UNIQUE_ID),
	CATEGORY varchar(MAX) NOT NULL
);

CREATE TABLE THEMES (
	THEME_ID int IDENTITY(1, 1) PRIMARY KEY,
	THEME varchar(20) UNIQUE NOT NULL
);

CREATE TABLE LANGS (
	LANG_ID int IDENTITY(1, 1) PRIMARY KEY,
	LANG varchar(2) UNIQUE NOT NULL
);

CREATE TABLE SETTINGS (
	USER_UID int UNIQUE CONSTRAINT SETT_USR_ID FOREIGN KEY REFERENCES USERS(UNIQUE_ID),
	ACTIVE_THEME int CONSTRAINT SETT_THM_ID FOREIGN KEY REFERENCES THEMES(THEME_ID) default(1),
	ACTIVE_LANG int CONSTRAINT SETT_LANG_ID FOREIGN KEY REFERENCES LANGS(LANG_ID) default(1),
	CARDS_LIMIT int CHECK (CARDS_LIMIT >= 5 AND CARDS_LIMIT <= 100) default(5),
	SWITCHED_REVIEW varchar(5) CHECK (SWITCHED_REVIEW IN ('True', 'False')) default('False'),
	TIME_LIMIT int CHECK (TIME_LIMIT >= 0 AND TIME_LIMIT <= 120) default(0)
);

CREATE TABLE FOLDERS (
	FOLDER_ID int IDENTITY(1, 1) PRIMARY KEY,
	USER_UID int CONSTRAINT FOLDER_USR_ID FOREIGN KEY REFERENCES USERS(UNIQUE_ID),
	FOLDER_NAME varchar(MAX) NOT NULL,
	CREATED_DATE datetime NOT NULL,
	CATEGORY varchar(MAX) default('none')
);

--CREATE TABLE CARDS_PACKS (
--	CARDS_ID int IDENTITY(1, 1) PRIMARY KEY,
--	FOLDER_ID int CONSTRAINT CARDS_FOLDER_ID FOREIGN KEY REFERENCES FOLDERS(FOLDER_ID)
--	--CARD_ID
--);

CREATE TABLE CARDS (
	-- combined primary key: folder_id and term
	FOLDER_ID int CONSTRAINT CARD_FOLDER_ID FOREIGN KEY REFERENCES FOLDERS(FOLDER_ID),
	--CARDS_PACK_ID int CONSTRAINT CARD_CARDS_ID FOREIGN KEY REFERENCES CARDS(CARDS_ID),
	CREATED_DATE datetime NOT NULL,
	TERM varchar(MAX) NOT NULL,
	TRANSLATION varchar(MAX) NOT NULL,
	EXAMPLES varchar(MAX) default(''),
	IS_MEMORIZED varchar(5) CHECK (IS_MEMORIZED IN ('True', 'False')) default('False'),
	RIGHT_ANSWERS int CHECK (RIGHT_ANSWERS >= 0) default(0),
	WRONG_ANSWERS int CHECK (WRONG_ANSWERS >= 0) default(0)
);

INSERT INTO LANGS VALUES ('EN'), ('RU'), ('JP');
INSERT INTO THEMES VALUES ('LIGHT'), ('DARK');