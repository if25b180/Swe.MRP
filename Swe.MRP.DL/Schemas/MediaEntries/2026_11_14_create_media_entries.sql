CREATE TYPE media_entry_type
    AS ENUM ('game', 'movie', 'series'); -- maps to MediaEntry.cs->enum MediaEntryType

CREATE TABLE media_entries (
    id SERIAL PRIMARY KEY,

    media_type media_entry_type NOT NULL
        DEFAULT 'game',

    title TEXT NOT NULL UNIQUE
        CHECK (btrim(title) <> ''),

    description TEXT
        DEFAULT '',

    release_year INTEGER NOT NULL
        DEFAULT 0 CHECK (release_year >= 0),

    age_restriction INTEGER NOT NULL
        DEFAULT 0 CHECK (age_restriction >= 0)
);