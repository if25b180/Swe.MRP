CREATE TABLE ratings (
    rating_id SERIAL PRIMARY KEY,

    fk_user_id INTEGER NOT NULL
        REFERENCES users (user_id)
        ON DELETE CASCADE,

    fk_media_entry_id INTEGER NOT NULL
        REFERENCES media_entries (media_entry_id)
        ON DELETE CASCADE,

    stars INTEGER NOT NULL
        CHECK (stars BETWEEN 1 AND 5),

    comment TEXT,

    approved BOOLEAN NOT NULL
        DEFAULT FALSE,

    UNIQUE (fk_user_id, fk_media_entry_id)
);