"""Draws the PDF Reader app icon. Writes Assets/icon-1024.png and Assets/AppIcon.ico (requires Pillow)."""
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

OUT = Path(__file__).resolve().parent.parent / "Assets"
S = 2048  # draw at 2x, downsample to 1024
k = S / 1024


def px(v):
    return int(round(v * k))


def gradient(size, top, bottom):
    mask = Image.linear_gradient("L").resize(size)
    return Image.composite(Image.new("RGBA", size, bottom), Image.new("RGBA", size, top), mask)


def rounded_mask(box, radius):
    m = Image.new("L", (S, S), 0)
    ImageDraw.Draw(m).rounded_rectangle([px(v) for v in box], px(radius), fill=255)
    return m


def shadow(canvas, mask, offset, blur, alpha):
    sh = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    sh.putalpha(mask.point(lambda v: v * alpha // 255))
    sh = ImageChops.offset(sh, px(offset[0]), px(offset[1])).filter(ImageFilter.GaussianBlur(px(blur)))
    canvas.alpha_composite(sh)


def paste_fill(canvas, fill, mask):
    canvas.paste(fill, (0, 0), mask)


img = Image.new("RGBA", (S, S), (0, 0, 0, 0))

# --- rounded-square background: deep blue gradient + soft top highlight
bg_mask = rounded_mask((32, 32, 992, 992), 220)
bg = gradient((S, S), (38, 122, 255, 255), (7, 33, 105, 255))
glow = Image.new("RGBA", (S, S), (0, 0, 0, 0))
ImageDraw.Draw(glow).ellipse([px(-120), px(-420), px(1144), px(520)], fill=(120, 190, 255, 110))
glow = glow.filter(ImageFilter.GaussianBlur(px(70)))
bg.alpha_composite(glow)
paste_fill(img, bg, bg_mask)

# subtle inner rim light
rim = ImageChops.subtract(bg_mask, bg_mask.filter(ImageFilter.MinFilter(px(7) | 1)))
rim_layer = Image.new("RGBA", (S, S), (170, 215, 255, 120))
img.paste(rim_layer, (0, 0), rim.filter(ImageFilter.GaussianBlur(px(1.5))))

# --- document sheet with folded top-right corner
L, T, R, B, F = 252, 138, 772, 862, 170  # fold size F
sheet_mask = Image.new("L", (S, S), 0)
ImageDraw.Draw(sheet_mask).polygon(
    [(px(L), px(T)), (px(R - F), px(T)), (px(R), px(T + F)), (px(R), px(B)), (px(L), px(B))], fill=255)
sheet_mask = ImageChops.multiply(sheet_mask, rounded_mask((L, T, R, B), 34))
# the rounded mask would re-round the cut corner; rebuild so only outer corners are rounded
cut = Image.new("L", (S, S), 255)
ImageDraw.Draw(cut).polygon(
    [(px(R - F), px(T - 1)), (px(R + 1), px(T - 1)), (px(R + 1), px(T + F))], fill=0)
sheet_mask = ImageChops.multiply(rounded_mask((L, T, R, B), 34), cut)

shadow(img, sheet_mask, (0, 26), 30, 150)
shadow(img, sheet_mask, (0, 6), 8, 90)
paste_fill(img, gradient((S, S), (255, 255, 255, 255), (221, 232, 248, 255)), sheet_mask)

# fold flap
flap_mask = Image.new("L", (S, S), 0)
ImageDraw.Draw(flap_mask).polygon(
    [(px(R - F), px(T)), (px(R - F), px(T + F - 18)), (px(R - F + 18), px(T + F)), (px(R), px(T + F))], fill=255)
shadow(img, flap_mask, (-4, 8), 8, 110)
paste_fill(img, gradient((S, S), (190, 214, 245, 255), (233, 241, 252, 255)), flap_mask)

# placeholder text lines on the sheet
d = ImageDraw.Draw(img)
for i, w in enumerate((270, 250, 300)):
    y = 232 + i * 52
    d.rounded_rectangle([px(L + 52), px(y), px(L + 52 + w), px(y + 20)], px(10), fill=(176, 196, 226, 255))

# --- red PDF badge, overlapping the lower-left of the sheet
bl, bt, br, bb = 138, 560, 664, 800
badge_mask = rounded_mask((bl, bt, br, bb), 46)
shadow(img, badge_mask, (0, 28), 26, 170)
badge = gradient((S, S), (255, 92, 84, 255), (190, 18, 32, 255))
paste_fill(img, badge, badge_mask)
# glossy top sheen on the badge
sheen = Image.new("RGBA", (S, S), (0, 0, 0, 0))
ImageDraw.Draw(sheen).rounded_rectangle(
    [px(bl + 6), px(bt + 6), px(br - 6), px(bt + (bb - bt) * 0.48)], px(40), fill=(255, 255, 255, 46))
sheen.putalpha(ImageChops.multiply(sheen.getchannel("A"), badge_mask))
img.alpha_composite(sheen)

font = ImageFont.truetype(r"C:\Windows\Fonts\seguibl.ttf", px(168))
text = "PDF"
box = d.textbbox((0, 0), text, font=font)
tw, th = box[2] - box[0], box[3] - box[1]
cx, cy = px((bl + br) / 2), px((bt + bb) / 2)
tx, ty = cx - tw / 2 - box[0], cy - th / 2 - box[1]
tshadow = Image.new("RGBA", (S, S), (0, 0, 0, 0))
ImageDraw.Draw(tshadow).text((tx, ty + px(5)), text, font=font, fill=(90, 0, 10, 140))
img.alpha_composite(tshadow.filter(ImageFilter.GaussianBlur(px(4))))
d.text((tx, ty), text, font=font, fill=(255, 255, 255, 255))

# --- export
final = img.resize((1024, 1024), Image.LANCZOS)
final.save(OUT / "icon-1024.png")
sizes = [16, 20, 24, 32, 40, 48, 64, 128, 256]
final.save(OUT / "AppIcon.ico", sizes=[(s, s) for s in sizes])
print("wrote", OUT / "AppIcon.ico")
