# Optional release ringtone

Daily Quest builds and runs without a local ringtone file. In that case, timer expiry uses alternating Windows system sounds.

The official Windows release embeds Mixkit's **Facility alarm sound** as part of the compiled application. The Mixkit license does not allow the raw item to be redistributed with source files, so the WAV is intentionally excluded from this repository.

To reproduce the official release audio locally:

1. Download **Facility alarm sound** directly from [Mixkit's alarm sound-effects page](https://mixkit.co/free-sound-effects/alarm/).
2. Save it in this directory as `mixkit-facility-alarm-sound-999.wav`.
3. Build or publish Daily Quest normally.

See [Third-Party Notices](../../THIRD_PARTY_NOTICES.md) and the [Mixkit Sound Effects Free License](https://mixkit.co/license/).
