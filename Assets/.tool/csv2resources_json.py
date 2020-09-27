import argparse
import csv
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
        "input": "./csv/skill.csv",
        "output": "../Resources/Json/Skill/",
        "name": "Skill",
        "template": "./csv/template_skill.csv",
        },
    Resource.EQUIP : {
        "input": "./csv/equip.csv",
        "output": "../Resources/Json/Equip/",
        "name": "Equip",
        "template": "./csv/template_equip.csv",
        },
    Resource.CHARACTOR : {
        "input": "./csv/charactor.csv",
        "output": "../Resources/Json/Charactor/",
        "name": "Charactor",
        "template": "./csv/template_charactor.csv",
        },
    Resource.PARTYSETS : {
        "input": "./csv/partySets.csv",
        "output": "../Resources/Json/PartySets/",
        "name": "PartySets",
        "template": "./csv/template_partySets.csv",
        },
    # Resource.ITEM : {
    #   "input": "./csv/item.csv",
    #   "output": "../Resources/Json/Item/",
    #   "name": "Item",
    #   "template": "./csv/template_item.csv",
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
    STR = "str"
    INT = "int"
    FROAT = "float"
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
CSV_FORMAT = {
    Resource.SKILL:
        "file_id,name,key,req.h,req.m,req.f,req.i",
    Resource.EQUIP: 
        "file_id,name,role,inoperative,operative,space,self,lvr,dur.p,dur.growrateKey,\
        HP.p,HP.growrateKey,MOV.p,MOV.growrateKey,CP.p,CP.growrateKey,DEX.p,DEX.growrateKey,\
        RNGmin.p,RNGmin.growrateKey,RNGmax.p,RNGmax.growrateKey,SIGmin.p,SIGmin.growrateKey,SIGmax.p,SIGmax.growrateKey",
    Resource.CHARACTOR:
        "file_id,race,role,equip,skilltree.h,skilltree.m,skilltree.f,skilltree.i,INT,HP,MOV,CP,DEX,RNGmin,RNGmax,SIGmin,SIGmax",
    Resource.PARTYSETS:
        "file_id,partyid,members,skilltree.h,skilltree.m,skilltree.f,skilltree.i,lvr,lvg,lvc",
    # Resource.ITEM: "",
}

def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument('-r', '--resource_type', type=int, default=0)
    parser.add_argument('--make_template', action="store_true")
    parser.add_argument('-rm', '--remove_json_in_resources', action="store_true")
    parser.add_argument('--indent', type=int, default=2, help="output json indent. if -1 then indent=None")
    args = parser.parse_args()
    return args

def csv2json_by_1_line(res: Resource, indent: int):
    with open(RESOURCE2KEYS[res]["input"], 'r') as f:
        data = csv.DictReader(f)
        for row in data:
            format_csv4json(res, row, indent)
def format_csv4json(res: Resource, csv_1_line: OrderedDict, indent: int):
    p = OrderedDict()
    filename = ""
    for k in list(csv_1_line.keys()):
        keytree: list = k.split(".")
        if keytree[0] == "file_id":  # skip
            filename = (str)(csv_1_line[k])
            continue
        p = cast_csv2dict(FORMAT[res], csv_1_line, keytree, k, 0, p)

        # datatype = FORMAT[res][keytree[0]]
        # if len(keytree) > 1:
        #     sp: OrderedDict = p.get(keytree[0], OrderedDict())
        #     sp[keytree[1]] = (float)(csv_1_line[k]) if keytree[1] == "p" else (int)(csv_1_line[k])
        #     p[keytree[0]] = sp
        # elif datatype == Format.STR:
        #     p[keytree[0]] = (str)(csv_1_line[k])
        # elif datatype == Format.INT:
        #     p[keytree[0]] = (int)(csv_1_line[k])
        # elif datatype == Format.FROAT:
        #     p[keytree[0]] = (float)(csv_1_line[k])
        # elif datatype == Format.STRS:
        #     p[keytree[0]] = csv_1_line[k].split(" - ")
    
    json_dump(RESOURCE2KEYS[res]["output"] + filename + ".json", p, indent)
    SUMMARY_FORMAT[RESOURCE2KEYS[res]["name"]].append(filename)
    
def cast_csv2dict(format_dict: dict, line: OrderedDict, keytree: list, k: str, n: int, ret: OrderedDict):
    if len(keytree) > n+1:
        sp: OrderedDict = ret.get(keytree[n], OrderedDict())
        ret[keytree[n]] = cast_csv2dict(format_dict[keytree[n]],line, keytree, k, n+1, ret.get(keytree[n], OrderedDict()))
    elif format_dict[keytree[n]] == Format.STR:
        ret[keytree[n]] = (str)(line[k])
    elif format_dict[keytree[n]] == Format.INT:
        ret[keytree[n]] = (int)(line[k])
    elif format_dict[keytree[n]] == Format.FROAT:
        ret[keytree[n]] = (float)(line[k])
    elif format_dict[keytree[n]] == Format.STRS:
        ret[keytree[n]] = line[k].split(" - ")
    return ret


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
# def make_template(indent: int):
#     for res in Resource:
#         d = OrderedDict({
#             "file_id1": FORMAT[res],
#             "file_id2": FORMAT[res],
#             "file_id3": FORMAT[res],
#             "file_id4": FORMAT[res],
#             }
#         )
#         json_dump(RESOURCE2KEYS[res]["template"], d, indent)

def main():
    args = parse_args()
    indent = args.indent if args.indent != -1 else None

    if args.remove_json_in_resources:
        remove_by_resource()

    # elif args.make_template:
    #     make_template(indent)

    elif args.resource_type == 0:
        for res in Resource:
            csv2json_by_1_line(res, indent)
        json_dump(SUMMARY_PATH, SUMMARY_FORMAT, indent)

    else:
        csv2json_by_1_line(Resource(args.resource_type), indent)

if __name__ == "__main__":
    main()
