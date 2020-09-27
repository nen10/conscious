"""split .json file for Unity JsonUtility"""
import argparse
import glob
from enum import IntEnum
import json
import os
from collections import OrderedDict

class Resource(IntEnum):
    SKILL = 1
    EQUIP = 2
    CHARACTOR = 3
    PARTYSETS = 4
    # ITEM = 5
RESOURCE2KEYS = {
    Resource.SKILL: {
        "input": "./json/skill.json",
        "output": "../Resources/Json/Skill/",
        "name": "Skill",
        "template": "./json/template_skill.json",
        },
    Resource.EQUIP : {
        "input": "./json/equip.json",
        "output": "../Resources/Json/Equip/",
        "name": "Equip",
        "template": "./json/template_equip.json",
        },
    Resource.CHARACTOR : {
        "input": "./json/charactor.json",
        "output": "../Resources/Json/Charactor/",
        "name": "Charactor",
        "template": "./json/template_charactor.json",
        },
    Resource.PARTYSETS : {
        "input": "./json/partySets.json",
        "output": "../Resources/Json/PartySets/",
        "name": "PartySets",
        "template": "./json/template_partySets.json",
        },
    # Resource.ITEM : {
    #   "input": "./json/item.json",
    #   "output": "../Resources/Json/Item/",
    #   "name": "Item",
    #   "template": "./json/template_item.json",
    #   },
}
SUMMARY_PATH = "../Resources/Json/summary.json"
SUMMARY_FORMAT = OrderedDict({
    "Skill": [],
    "Equip": [],
    "Charactor": [],
    "PartySets": [],
    # "Item": [],
})
class Format:
    STR = ""
    INT = 0
    FROAT = 0.0
    STRS = ["",]

FORMAT = {
    Resource.SKILL:{
        "name": Format.STR,
        "key": Format.INT,
        "req": {"h": Format.INT, "m": Format.INT, "f": Format.INT, "i": Format.INT,}
    },
    Resource.EQUIP: {
        "name": Format.STR,
        "role": Format.INT,
        "inoperative": Format.STR,
        "operative": Format.STR,
        "space": Format.STR,
        "self": Format.STR,
        "lvr": Format.INT,
        "dur": {"p": Format.FROAT, "growrateKey": Format.INT,},
        "HP": {"p": Format.FROAT, "growrateKey": Format.INT,},
        "MOV": {"p": Format.FROAT, "growrateKey": Format.INT,},
        "CP": {"p": Format.FROAT, "growrateKey": Format.INT,},
        "DEX": {"p": Format.FROAT, "growrateKey": Format.INT,},
        "RNGmin": {"p": Format.FROAT, "growrateKey": Format.INT,},
        "RNGmax": {"p": Format.FROAT, "growrateKey": Format.INT,},
        "SIGmin": {"p": Format.FROAT, "growrateKey": Format.INT,},
        "SIGmax": {"p": Format.FROAT, "growrateKey": Format.INT,},
    },
    Resource.CHARACTOR: {
        "race": Format.STR,
        "role": Format.INT,
        "equip": Format.STR,
        "skilltree": {"h": Format.INT, "m": Format.INT, "f": Format.INT, "i": Format.INT,},
        "INT": Format.INT,
        "HP": Format.FROAT,
        "MOV": Format.FROAT,
        "CP": Format.FROAT,
        "DEX": Format.FROAT,
        "RNGmin": Format.FROAT,
        "RNGmax": Format.FROAT,
        "SIGmin": Format.FROAT,
        "SIGmax": Format.FROAT,
    },
    Resource.PARTYSETS : {
        "partyid": Format.INT,
        "members": Format.STRS,
        "skilltree": {"h": Format.INT, "m": Format.INT, "f": Format.INT, "i": Format.INT,},
        "lvr": Format.INT,
        "lvg": Format.INT,
        "lvc": Format.INT,
    },
    # Resource.ITEM : {},
}

def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument('-r', '--resource_type', type=int, default=0)
    parser.add_argument('--make_template', action="store_true")
    parser.add_argument('-rm', '--remove_json_in_resources', action="store_true")
    parser.add_argument('--indent', type=int, default=2, help="output json indent. if -1 then indent=None")
    args = parser.parse_args()
    return args

def json_split(res: Resource, indent: int):
    with open(RESOURCE2KEYS[res]["input"]) as f:
        od: OrderedDict = json.load(f, object_pairs_hook=OrderedDict)

    for k in list(od.keys()) :
        json_dump(RESOURCE2KEYS[res]["output"] + k + ".json", od[k], indent)
    SUMMARY_FORMAT[RESOURCE2KEYS[res]["name"]] = list(od.keys())

def json_dump(p: str, d: OrderedDict, indent: int):
    with open(p, 'w') as f:
        json.dump(d, f, indent=indent)


def remove_by_resource():
    for p in glob.glob("../Resources/Json/**/*.json", recursive=True):
        if os.path.isfile(p):
            os.remove(p)
    for p in glob.glob("../Resources/Json/**/*.json.meta", recursive=True):
        if os.path.isfile(p):
            os.remove(p)
def make_template(indent: int):
    for res in Resource:
        d = OrderedDict({
            "file_id1": FORMAT[res],
            "file_id2": FORMAT[res],
            "file_id3": FORMAT[res],
            "file_id4": FORMAT[res],
            }
        )
        json_dump(RESOURCE2KEYS[res]["template"], d, indent)

def main():
    args = parse_args()
    indent = args.indent if args.indent != -1 else None

    if args.remove_json_in_resources:
        remove_by_resource()

    elif args.make_template:
        make_template(indent)

    elif args.resource_type == 0:
        for res in Resource:
            json_split(res, indent)
        json_dump(SUMMARY_PATH, SUMMARY_FORMAT, indent)

    else:
        json_split(Resource(args.resource_type), indent)

if __name__ == "__main__":
    main()
