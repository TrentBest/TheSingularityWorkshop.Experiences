#!/usr/bin/env python3
"""Dependency-free structural checks for the living Experience documentation."""
from __future__ import annotations

from html.parser import HTMLParser
from pathlib import Path
import subprocess
import tempfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "docs" / "assets"
DEMOS = ROOT / "docs" / "demos"


class DocumentProbe(HTMLParser):
    def __init__(self) -> None:
        super().__init__(convert_charrefs=True)
        self.ids: list[str] = []
        self.scripts: list[str] = []
        self._script_depth = 0
        self._script_parts: list[str] = []
        self.buttons: list[dict[str, str]] = []
        self.select_ids: list[str] = []

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        values = {key: value or "" for key, value in attrs}
        if values.get("id"):
            self.ids.append(values["id"])
        if tag == "script":
            self._script_depth += 1
            self._script_parts = []
        if tag == "button":
            self.buttons.append(values)
        if tag == "select" and values.get("id"):
            self.select_ids.append(values["id"])

    def handle_data(self, data: str) -> None:
        if self._script_depth:
            self._script_parts.append(data)

    def handle_endtag(self, tag: str) -> None:
        if tag == "script" and self._script_depth:
            self.scripts.append("".join(self._script_parts))
            self._script_parts = []
            self._script_depth -= 1


def require(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(f"Living documentation validation failed: {message}")


def parse_svg(name: str) -> str:
    path = ASSETS / name
    root = ET.parse(path).getroot()
    require(root.tag == "{http://www.w3.org/2000/svg}svg", f"{name} root must be SVG")
    children = {child.tag.rsplit("}", 1)[-1] for child in root}
    require("title" in children and "desc" in children, f"{name} must have accessible title and description")
    source = path.read_text(encoding="utf-8")
    require("{#" not in source, f"{name} contains an unexpanded template directive")
    return source


def parse_demo(name: str) -> tuple[str, DocumentProbe]:
    path = DEMOS / name
    source = path.read_text(encoding="utf-8")
    probe = DocumentProbe()
    probe.feed(source)
    probe.close()
    require(len(probe.ids) == len(set(probe.ids)), f"{name} contains duplicate HTML IDs")
    require(len(probe.scripts) == 1, f"{name} must have exactly one inline script")
    require("<!doctype html>" in source.lower(), f"{name} must declare HTML5")
    return source, probe


casino_svg = parse_svg("casino-floor.svg")
require("RECONCILIATION" in casino_svg and "BUY-IN" in casino_svg and "CASH-OUT" in casino_svg,
        "casino SVG must show accounting and distinct value flows")

casino, casino_probe = parse_demo("casino-floor.html")
require({"cashBalance", "cageCash", "cageInventory", "chips", "wagers", "returns", "reconcile", "journal"}.issubset(casino_probe.ids),
        "casino demo is missing a visible ledger/operator field")
require({"buyin", "wager", "payout", "cashout", "reset"}.issubset(
    {button.get("data-kind", "") for button in casino_probe.buttons}),
    "casino demo must expose buy-in, wager, payout, cash-out, and reset controls")

readme = (ROOT / "README.md").read_text(encoding="utf-8")
require("docs/assets/casino-floor.svg" in readme and "docs/demos/casino-floor.html" in readme,
        "README must link the living casino visual and interactive demonstration")

node = subprocess.run(["node", "--version"], capture_output=True, text=True)
require(node.returncode == 0, "Node.js is required to syntax-check the interactive demo script")
with tempfile.TemporaryDirectory() as directory:
    js_path = Path(directory) / "casino-floor.js"
    js_path.write_text(casino_probe.scripts[0], encoding="utf-8")
    subprocess.run(["node", "--check", str(js_path)], check=True)

print("Living casino documentation checks passed: SVG XML/accessibility, demo structure, controls, and JavaScript syntax.")
