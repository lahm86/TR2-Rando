﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using TRLevelReader.Model;
using TRLevelReader.Serialization;

using Newtonsoft.Json;
using TRLevelReader;
using TRLevelReader.Model.Enums;

namespace TR2Randomizer
{
    public class SecretReplacer
    {
        private TR2LevelReader _reader;
        private TR2LevelWriter _writer;
        private TR2Level _levelInstance;
        private Random _generator;
        private List<string> _levels;

        public bool PlaceAll { get; set; }
        
        public SecretReplacer()
        {
            PlaceAll = false;

            //_levels = LevelNames.AsList;
            _levels = new List<string> { LevelNames.GW };

            _reader = new TR2LevelReader();
            _writer = new TR2LevelWriter();
        }

        private void RandomizeSecrets(List<Location> LevelLocations, string lvlname)
        {
            if (LevelLocations.Count > 2)
            {
                if (PlaceAll)
                {
                    PlaceAllSecrets(lvlname, LevelLocations);
                    return;
                }

                //Apply zoning to the locations to ensure they are spread out.
                ZonedLocationCollection ZonedLocations = AssignLocationsToZones(lvlname, LevelLocations);

                Location GoldSecret;
                Location JadeSecret;
                Location StoneSecret;

                //Find suitable locations, ensuring they are zoned, do not share a room and difficulty.
                do
                {
                    GoldSecret = ZonedLocations.ZoneOneLocations[_generator.Next(0, LevelLocations.Count)];
                } while (GoldSecret.Difficulty == Difficulty.Hard && ReplacementStatusManager.AllowHard == false);
                

                do
                {
                    JadeSecret = ZonedLocations.ZoneTwoLocations[_generator.Next(0, LevelLocations.Count)];
                } while ((JadeSecret.Room == GoldSecret.Room) || 
                        (JadeSecret.Difficulty == Difficulty.Hard && ReplacementStatusManager.AllowHard == false));

                do
                {
                    StoneSecret = ZonedLocations.ZoneThreeLocations[_generator.Next(0, LevelLocations.Count)];
                } while ((StoneSecret.Room == GoldSecret.Room) || 
                        (StoneSecret.Room == JadeSecret.Room) ||
                        (StoneSecret.Difficulty == Difficulty.Hard && ReplacementStatusManager.AllowHard == false));

                //Does the level contain the entities?
                int GoldIndex = Array.FindIndex(_levelInstance.Entities, ent => (ent.TypeID == (short)TR2Entities.GoldSecret_S_P));
                int JadeIndex = Array.FindIndex(_levelInstance.Entities, ent => (ent.TypeID == (short)TR2Entities.JadeSecret_S_P));
                int StoneIndex = Array.FindIndex(_levelInstance.Entities, ent => (ent.TypeID == (short)TR2Entities.StoneSecret_S_P));

                //Check if we could find instances of the secret entities, if not, we need to add not edit.
                if (GoldIndex != -1)
                {
                    _levelInstance.Entities[GoldIndex].Room = Convert.ToInt16(GoldSecret.Room);
                    _levelInstance.Entities[GoldIndex].X = GoldSecret.X;
                    _levelInstance.Entities[GoldIndex].Y = GoldSecret.Y;
                    _levelInstance.Entities[GoldIndex].Z = GoldSecret.Z;
                }
                else
                {
                    TR2Entity GoldEntity = new TR2Entity
                    {
                        TypeID = (int)TR2Entities.GoldSecret_S_P,
                        Room = Convert.ToInt16(GoldSecret.Room),
                        X = GoldSecret.X,
                        Y = GoldSecret.Y,
                        Z = GoldSecret.Z,
                        Angle = 0,
                        Intensity1 = -1,
                        Intensity2 = -1,
                        Flags = 0
                    };

                    _levelInstance.Entities.Append(GoldEntity);
                    _levelInstance.NumEntities++;
                }

                if (JadeIndex != -1)
                {
                    _levelInstance.Entities[JadeIndex].Room = Convert.ToInt16(JadeSecret.Room);
                    _levelInstance.Entities[JadeIndex].X = JadeSecret.X;
                    _levelInstance.Entities[JadeIndex].Y = JadeSecret.Y;
                    _levelInstance.Entities[JadeIndex].Z = JadeSecret.Z;
                }
                else
                {
                    TR2Entity JadeEntity = new TR2Entity
                    {
                        TypeID = (int)TR2Entities.JadeSecret_S_P,
                        Room = Convert.ToInt16(JadeSecret.Room),
                        X = JadeSecret.X,
                        Y = JadeSecret.Y,
                        Z = JadeSecret.Z,
                        Angle = 0,
                        Intensity1 = -1,
                        Intensity2 = -1,
                        Flags = 0
                    };

                    _levelInstance.Entities.Append(JadeEntity);
                    _levelInstance.NumEntities++;
                }

                if (StoneIndex != -1)
                {
                    _levelInstance.Entities[StoneIndex].Room = Convert.ToInt16(StoneSecret.Room);
                    _levelInstance.Entities[StoneIndex].X = StoneSecret.X;
                    _levelInstance.Entities[StoneIndex].Y = StoneSecret.Y;
                    _levelInstance.Entities[StoneIndex].Z = StoneSecret.Z;
                }
                else
                {
                    TR2Entity StoneEntity = new TR2Entity
                    {
                        TypeID = (int)TR2Entities.StoneSecret_S_P,
                        Room = Convert.ToInt16(StoneSecret.Room),
                        X = StoneSecret.X,
                        Y = StoneSecret.Y,
                        Z = StoneSecret.Z,
                        Angle = 0,
                        Intensity1 = -1,
                        Intensity2 = -1,
                        Flags = 0
                    };

                    _levelInstance.Entities.Append(StoneEntity);
                    _levelInstance.NumEntities++;
                }
            }
        }

        private void PlaceAllSecrets(string lvl, List<Location> LevelLocations)
        {
            
        }

        private ZonedLocationCollection AssignLocationsToZones(string lvl, List<Location> locations)
        {
            Dictionary<int, List<int>> ZoneMap = JsonConvert.DeserializeObject<Dictionary<int, List<int>>>(File.ReadAllText(Directory.GetCurrentDirectory() + "\\Zones\\" + lvl + "-Zones.json"));

            return new ZonedLocationCollection
            {
                ZoneOneLocations = (from loc in locations
                                    where ZoneMap[0].Contains(loc.Room)
                                    select loc).ToList(),

                ZoneTwoLocations = (from loc in locations
                                    where ZoneMap[1].Contains(loc.Room)
                                    select loc).ToList(),
                
                ZoneThreeLocations = (from loc in locations
                                      where ZoneMap[2].Contains(loc.Room)
                                      select loc).ToList()
            };
        }

        public void Replace(int seed)
        {
            ReplacementStatusManager.CanRandomize = false;

            _generator = new Random(seed);

            Dictionary<string, List<Location>> Locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText("locations.json"));

            foreach (string lvl in _levels)
            {
                //Read the level into a level object
                _levelInstance = _reader.ReadLevel(lvl);

                //Apply the modifications
                RandomizeSecrets(Locations[lvl], lvl);

                //Write back the level file
                _writer.WriteLevelToFile(_levelInstance, lvl);

                ReplacementStatusManager.LevelProgress++;
            }

            ReplacementStatusManager.LevelProgress = 0;
            ReplacementStatusManager.CanRandomize = true;
        }
    }
}
