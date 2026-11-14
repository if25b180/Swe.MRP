CREATE TABLE media_genres (
    fk_media_entry_id INTEGER NOT NULL
        REFERENCES media_entries (id) ON DELETE CASCADE,

    fk_genre_id INTEGER NOT NULL
        REFERENCES genres (id) ON DELETE CASCADE,

    PRIMARY KEY (fk_media_entry_id, fk_genre_id)
);