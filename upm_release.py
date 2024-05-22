# Sets up upm folder with a Unity importable version of the source.
# Requires: sibling dir "upm" set up as a git clone, checked out to branch "upm".

import os
import shutil
import re
from typing import List, IO, Tuple

UPM_DIR="../upm/"

PACKAGE_JSON="""{
  "name": "com.runevision.layerprocgen",
  "version": "VERSION",
  "displayName": "LayerProcGen",
  "description": "LayerProcGen is a framework that can be used to implement layer-based procedural generation that's infinite, deterministic and contextual.",
  "license": "MPL-2.0",
  "unity": "2019.4",
  "documentationUrl": "https://runevision.github.io/LayerProcGen/",
  "dependencies": {
  },
  "samples": [
    {
      "displayName": "Simple Samples",
      "description": "Contains a few simple sample scenes including scripts.",
      "path": "Samples~/SimpleSamples"
    },
    {
      "displayName": "Terrain Sample",
      "description": "Contains a sample scene, including scripts, for generating a terrain with natural paths.",
      "path": "Samples~/TerrainSample"
    }
  ],
  "keywords": [
    "procedural",
    "generation"
  ],
  "author": {
    "name": "Rune Skovbo Johansen",
    "email": "rune@runevision.com",
    "url": "https://runevision.com"
  }
}
"""

META_FILE="""fileFormatVersion: 2
guid: GUID
PackageManifestImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

def get_version():
    is_preview = False
    for line in open("CHANGELOG.md", "r").readlines():
        if line.startswith("##"):
            version = line[2:].strip().lower().lstrip("v")
            if version == "unreleased":
                is_preview = True
                continue
            if not re.fullmatch("\d+\.\d+.\d", version):
                raise Exception(f"Version doesn't appear to be semver: {version}")
            return version + ("-preview" if is_preview else "")

    raise Exception("Couldn't find version")

def build_upm_release():
    # Copy source data
    ignored = shutil.ignore_patterns()
    shutil.copytree("Src/", UPM_DIR, ignore=ignored, dirs_exist_ok=True)
    shutil.copytree("Samples/", UPM_DIR + "Samples~/", ignore=ignored, dirs_exist_ok=True)

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
    open(UPM_DIR + "README.md", "w").write("\n\n<!-- THIS FILE IS AUTO-GENERATED FROM THE DOCS FRONT PAGE -->\n\n\n" + readme)

    # Copy other files
    shutil.copy("CHANGELOG.md", UPM_DIR + "CHANGELOG.md")
    shutil.copy("CHANGELOG.md.meta", UPM_DIR + "CHANGELOG.md.meta")
    shutil.copy("LICENSE.md", UPM_DIR + "LICENSE.md")
    shutil.copy("LICENSE.md.meta", UPM_DIR + "LICENSE.md.meta")
    #shutil.copy("README.md", UPM_DIR + "README.md")
    shutil.copy("README.md.meta", UPM_DIR + "README.md.meta")
    shutil.copy("Third Party Notices.md", UPM_DIR + "Third Party Notices.md")
    shutil.copy("Third Party Notices.md.meta", UPM_DIR + "Third Party Notices.md.meta")

    # Create package.json file
    package_json = PACKAGE_JSON.replace("VERSION", get_version())
    package_json_meta = META_FILE.replace("GUID", "f1654b95596bb49ad94c8ef46ca50459")
    open(UPM_DIR + "package.json", "w").write(package_json)
    open(UPM_DIR + "package.json.meta", "w").write(package_json_meta)


if __name__ == "__main__":
    build_upm_release()