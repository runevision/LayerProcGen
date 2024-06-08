#!/usr/bin/env python3
# Sets up godot_project folder with a Unity importable version of the source.
# Requires: sibling dir "godot_project" set up as a git clone, checked out to branch "godot_project".

import os
import shutil
import re
from typing import List, IO, Tuple

PROJECT_DIR="../godot_project/"
ADDON_DIR=PROJECT_DIR + "/addons/LayerProcGen/"

PLUGIN_CFG="""
[plugin]

name="LayerProcGen"
description="LayerProcGen is a framework that can be used to implement layer-based procedural generation that's infinite, deterministic and contextual."
author="Rune Skovbo Johansen - rune@runevision.com"
contributor="Sythelux Rikd - dersyth@gmail.com"
version="VERSION"
script="LayerProcGen.cs"
language="C-sharp"
documentationUrl="https://runevision.github.io/LayerProcGen/"
"""

def get_version():
    is_preview = False
    for line in open("CHANGELOG.md", "r").readlines():
        if line.startswith("##"):
            version = line[2:].strip().lower().lstrip("v").split(" ")[0]
            if version == "unreleased":
                is_preview = True
                continue
            if not re.fullmatch("\d+\.\d+.\d", version):
                raise Exception(f"Version doesn't appear to be semver: {version}")
            return version + ("-preview" if is_preview else "")

    raise Exception("Couldn't find version")

def build_addon_release():
    global PLUGIN_CFG
    version = get_version()
    # Copy source data
    ignored = shutil.ignore_patterns("*.meta", "*.asmdef")
    shutil.copytree("Src/", ADDON_DIR, ignore=ignored, dirs_exist_ok=True)
    # shutil.copytree("Samples/", PROJECT_DIR + "Samples/"+version+"/", ignore=ignored, dirs_exist_ok=True) # Samples stay in that repo

    # Create repo readme from documentation front page
    # For this one, images should point to local ones in repo, since GitHub otherwise doesn't support large ones.
    readme = open("Documentation/README.md", "r").read()
    # Fix image paths
    readme = re.sub("\(./([^/]*).png", "(Documentation/\\1.png", readme)
    readme = re.sub("\(./([^/]*).gif", "(Documentation/\\1.gif", readme)
    # Reroute local links to online html docs
    readme = readme.replace("(./", "(https://runevision.github.io/LayerProcGen/")
    # Change links to other markdown pages to links to generated html pages
    readme = re.sub("/([^/]*).md", "/md_\\1.html", readme)
    # Write file with warning at top
    open("README.md", "w").write("\n\n<!-- THIS FILE IS AUTO-GENERATED FROM THE DOCS FRONT PAGE -->\n\n\n" + readme)

    # Create UPM readme from documentation front page
    # For this one, images should point to ones in online documentation.
    readme = open("Documentation/README.md", "r").read()
    # Reroute local links to online html docs
    readme = readme.replace("(./", "(https://runevision.github.io/LayerProcGen/")
    # Change links to other markdown pages to links to generated html pages
    readme = re.sub("/([^/]*).md", "/md_\\1.html", readme)
    # Write file with warning at top
    open(PROJECT_DIR + "README.md", "w").write("\n\n<!-- THIS FILE IS AUTO-GENERATED FROM THE DOCS FRONT PAGE -->\n\n\n" + readme)

    # Copy other files
    shutil.copy("CHANGELOG.md", ADDON_DIR + "CHANGELOG.md")
    shutil.copy("LICENSE.md", ADDON_DIR + "LICENSE.md")
    shutil.copy("Third Party Notices.md", ADDON_DIR + "Third Party Notices.md")

    # Create package.json file
    PLUGIN_CFG = PLUGIN_CFG.replace("VERSION", version)
    open(ADDON_DIR + "plugin.cfg", "w").write(PLUGIN_CFG)


if __name__ == "__main__":
    build_addon_release()