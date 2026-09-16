"""Generate the Daily Quest liquid-glass logo and Windows icon assets."""

from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "Assets" / "DailyQuest-logo.png"
PREVIEW = ROOT / "Assets" / "DailyQuest-icon.png"
WINDOWS_ICON = ROOT / "Assets" / "DailyQuest.ico"
ICON_SIZES = [(16, 16), (20, 20), (24, 24), (32, 32), (40, 40), (48, 48), (64, 64), (128, 128), (256, 256)]

CANVAS_SIZE = 1024
RENDER_SCALE = 2


def scaled(value: int | float) -> int:
    return round(value * RENDER_SCALE)


def rounded_mask(size: int, box: tuple[int, int, int, int], radius: int) -> Image.Image:
    mask = Image.new("L", (size, size), 0)
    draw = ImageDraw.Draw(mask)
    draw.rounded_rectangle(tuple(scaled(value) for value in box), radius=scaled(radius), fill=255)
    return mask


def vertical_gradient(size: int, top: tuple[int, int, int], bottom: tuple[int, int, int]) -> Image.Image:
    image = Image.new("RGBA", (size, size))
    draw = ImageDraw.Draw(image)
    for y in range(size):
        ratio = y / max(1, size - 1)
        color = tuple(round(start + (end - start) * ratio) for start, end in zip(top, bottom))
        draw.line((0, y, size, y), fill=(*color, 255))
    return image


def add_blurred_ellipse(
    target: Image.Image,
    box: tuple[int, int, int, int],
    color: tuple[int, int, int, int],
    blur_radius: int,
) -> None:
    glow = Image.new("RGBA", target.size, (0, 0, 0, 0))
    ImageDraw.Draw(glow).ellipse(tuple(scaled(value) for value in box), fill=color)
    glow = glow.filter(ImageFilter.GaussianBlur(scaled(blur_radius)))
    target.alpha_composite(glow)


def draw_round_line(
    draw: ImageDraw.ImageDraw,
    points: list[tuple[int, int]],
    fill: int | tuple[int, int, int, int],
    width: int,
) -> None:
    scaled_points = [(scaled(x), scaled(y)) for x, y in points]
    scaled_width = scaled(width)
    radius = scaled_width // 2
    draw.line(scaled_points, fill=fill, width=scaled_width, joint="curve")
    for x, y in scaled_points:
        draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=fill)


def build_logo() -> Image.Image:
    size = scaled(CANVAS_SIZE)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))

    tile_box = (68, 68, 956, 956)
    tile_radius = 220
    tile_mask = rounded_mask(size, tile_box, tile_radius)

    shadow = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    shadow_draw = ImageDraw.Draw(shadow)
    shadow_draw.rounded_rectangle(
        tuple(scaled(value) for value in (74, 92, 950, 974)),
        radius=scaled(tile_radius),
        fill=(23, 33, 43, 64),
    )
    shadow = shadow.filter(ImageFilter.GaussianBlur(scaled(44)))
    canvas.alpha_composite(shadow)

    surface = vertical_gradient(size, (251, 253, 255), (232, 246, 241))
    add_blurred_ellipse(surface, (-210, 475, 615, 1270), (109, 207, 169, 96), 118)
    add_blurred_ellipse(surface, (455, -250, 1250, 570), (119, 191, 255, 88), 122)
    add_blurred_ellipse(surface, (500, 530, 1240, 1260), (179, 157, 219, 63), 130)

    shine = Image.new("RGBA", surface.size, (0, 0, 0, 0))
    shine_draw = ImageDraw.Draw(shine)
    shine_draw.ellipse(
        tuple(scaled(value) for value in (-250, -330, 840, 560)),
        fill=(255, 255, 255, 144),
    )
    shine = shine.filter(ImageFilter.GaussianBlur(scaled(96)))
    surface.alpha_composite(shine)
    surface.putalpha(tile_mask)
    canvas.alpha_composite(surface)

    edge = ImageDraw.Draw(canvas)
    edge.rounded_rectangle(
        tuple(scaled(value) for value in tile_box),
        radius=scaled(tile_radius),
        outline=(255, 255, 255, 220),
        width=scaled(12),
    )
    edge.rounded_rectangle(
        tuple(scaled(value) for value in (81, 81, 943, 943)),
        radius=scaled(207),
        outline=(255, 255, 255, 78),
        width=scaled(3),
    )

    check_shadow_mask = Image.new("L", canvas.size, 0)
    check_shadow_draw = ImageDraw.Draw(check_shadow_mask)
    draw_round_line(
        check_shadow_draw,
        [(270, 542), (430, 702), (760, 366)],
        fill=92,
        width=142,
    )
    check_shadow_mask = check_shadow_mask.filter(ImageFilter.GaussianBlur(scaled(22)))
    check_shadow = Image.new("RGBA", canvas.size, (23, 64, 48, 0))
    check_shadow.putalpha(check_shadow_mask)
    canvas.alpha_composite(check_shadow)

    check_mask = Image.new("L", canvas.size, 0)
    check_draw = ImageDraw.Draw(check_mask)
    draw_round_line(check_draw, [(270, 520), (430, 680), (760, 344)], fill=255, width=142)
    check = vertical_gradient(size, (71, 125, 103), (47, 98, 80))
    check.putalpha(check_mask)
    canvas.alpha_composite(check)

    check_glint_mask = Image.new("L", canvas.size, 0)
    check_glint_draw = ImageDraw.Draw(check_glint_mask)
    draw_round_line(check_glint_draw, [(287, 491), (430, 633), (735, 323)], fill=48, width=18)
    check_glint = Image.new("RGBA", canvas.size, (255, 255, 255, 0))
    check_glint.putalpha(check_glint_mask.filter(ImageFilter.GaussianBlur(scaled(5))))
    canvas.alpha_composite(check_glint)

    return canvas.resize((CANVAS_SIZE, CANVAS_SIZE), Image.Resampling.LANCZOS)


def build_small_icon(size: int) -> Image.Image:
    supersample = 8
    work_size = size * supersample
    image = Image.new("RGBA", (work_size, work_size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)

    margin = round(size * 0.075 * supersample)
    radius = round(size * 0.22 * supersample)
    draw.rounded_rectangle(
        (margin, margin, work_size - margin, work_size - margin),
        radius=radius,
        fill=(241, 248, 248, 255),
        outline=(255, 255, 255, 255),
        width=max(1, supersample),
    )

    points = [
        (round(size * 0.27 * supersample), round(size * 0.52 * supersample)),
        (round(size * 0.43 * supersample), round(size * 0.68 * supersample)),
        (round(size * 0.76 * supersample), round(size * 0.344 * supersample)),
    ]
    stroke = max(supersample, round(size * 0.142 * supersample))
    radius = stroke // 2
    draw.line(points, fill=(49, 95, 76, 255), width=stroke, joint="curve")
    for x, y in points:
        draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=(49, 95, 76, 255))

    return image.resize((size, size), Image.Resampling.LANCZOS)


def build_icon_frames(logo: Image.Image) -> list[Image.Image]:
    frames: list[Image.Image] = []
    for width, height in ICON_SIZES:
        if width != height:
            raise ValueError("Daily Quest icon frames must be square.")
        if width <= 24:
            frames.append(build_small_icon(width))
        else:
            frames.append(logo.resize((width, height), Image.Resampling.LANCZOS))
    return frames


def main() -> None:
    logo = build_logo()
    frames = build_icon_frames(logo)
    base_frame = frames[-1]

    logo.save(SOURCE, format="PNG", optimize=True)
    logo.save(PREVIEW, format="PNG", optimize=True)
    base_frame.save(
        WINDOWS_ICON,
        format="ICO",
        sizes=ICON_SIZES,
        append_images=frames[:-1],
        bitmap_format="png",
    )

    print(f"Created {SOURCE.relative_to(ROOT)}")
    print(f"Created {PREVIEW.relative_to(ROOT)}")
    print(f"Created {WINDOWS_ICON.relative_to(ROOT)} with {len(frames)} sizes")


if __name__ == "__main__":
    main()
