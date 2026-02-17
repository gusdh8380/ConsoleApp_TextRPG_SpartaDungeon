# Sparta Dungeon — Text RPG

A console-based text RPG built with **.NET 6.0** during the Unity bootcamp at Sparta Coding Club.

## About

Sparta Dungeon is a turn-based RPG where players create a character, manage equipment, trade at shops, and explore dungeons. Game state is automatically saved/loaded via JSON, so progress persists across sessions.

## Features

- **Character creation** — Choose name and class (Warrior / Rogue)
- **Status view** — Check level, HP, gold, and equipment bonuses
- **Inventory management** — Equip and unequip gear
- **Shop** — Buy and sell items
- **Rest** — Spend gold to recover HP
- **Dungeon exploration** — Difficulty-based damage and reward calculation
- **Auto save/load** — Game state saved to `TextRPG_Sparta.json` every turn

## Dev Log

Documented the design process and implementation challenges on Notion:
- [Development Log (Notion)](https://www.notion.so/TextRPG-_-SpartaDungeon-1d7dd79e4161809b9c0df64815fa2727?pvs=25)

## Tech Stack

| | |
|---|---|
| Framework | .NET 6.0 |
| Language | C# |
| Data | JSON (auto save/load) |
