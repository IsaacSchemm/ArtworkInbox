using System;

namespace ArtworkInbox.Inkbunny {
	public enum InkbunnyRating {
		General = 0,
		Mature = 1,
		Adult = 2
	}

	public enum InkbunnyRatingTag {
		Nudity = 2,
		Violence = 3,
		SexualThemes = 4,
		StrongViolence = 5
	}

    public enum InkbunnySubmissionType {
        Picture = 1,
        Sketch = 2,
        PictureSeries = 3,
        Comic = 4,
        Portfolio = 5,
        FlashAnimation = 6,
        FlashInteractive = 7,
        VideoFeatureLength = 8,
        VideoAnimation = 9,
        MusicTrack = 10,
        MusicAlbum = 11,
        WritingDocument = 12,
        CharacterSheet = 13,
        Photography = 14
    }

    public enum InkbunnyTwitterImagePref {
        TextOnly = 0,
        Thumbnail = 1,
        FullPicture = 2
    }
}