"""Build the multi-resolution Windows icon from the generated transparent logo."""

from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "Assets" / "DailyQuest-logo.png"
PREVIEW = ROOT / "Assets" / "DailyQuest-icon.png"
WINDOWS_ICON = ROOT / "Assets" / "DailyQuest.ico"
ICON_SIZES = [(16, 16), (20, 20), (24, 24), (32, 32), (40, 40), (48, 48), (64, 64), (128, 128), (256, 256)]


def build_square_mark(source: Image.Image) -> Image.Image:
    rgba = source.convert("RGBA")
    alpha = rgba.getchannel("A")
    bounds = alpha.getbbox()
    if bounds is None:
        raise ValueError("The source logo is completely transparent.")

    left, top, right, bottom = bounds
    mark_width = right - left
    mark_height = bottom - top
    padding = max(8, round(max(mark_width, mark_height) * 0.075))
    side = max(mark_width, mark_height) + padding * 2

    center_x = (left + right) / 2
    center_y = (top + bottom) / 2
    crop_left = round(center_x - side / 2)
    crop_top = round(center_y - side / 2)

    square = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    source_left = max(0, crop_left)
    source_top = max(0, crop_top)
    source_right = min(rgba.width, crop_left + side)
    source_bottom = min(rgba.height, crop_top + side)
    cropped = rgba.crop((source_left, source_top, source_right, source_bottom))
    destination_x = source_left - crop_left
    destination_y = source_top - crop_top
    square.alpha_composite(cropped, (destination_x, destination_y))
    return square


def main() -> None:
    with Image.open(SOURCE) as source:
        mark = build_square_mark(source)

    preview = mark.resize((1024, 1024), Image.Resampling.LANCZOS)
    preview.save(PREVIEW, format="PNG", optimize=True)
    preview.save(WINDOWS_ICON, format="ICO", sizes=ICON_SIZES, bitmap_format="png")

    print(f"Created {PREVIEW.relative_to(ROOT)}")
    print(f"Created {WINDOWS_ICON.relative_to(ROOT)} with {len(ICON_SIZES)} sizes")


if __name__ == "__main__":
    main()
