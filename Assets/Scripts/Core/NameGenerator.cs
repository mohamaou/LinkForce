using System;
using System.Collections.Generic;

public class NameGenerator
{
    private static readonly List<string> Names = new List<string>
    {
        "Aaron", "Abigail", "Adam", "Aiden", "Alex", "Alexa", "Alice", "Amber", "Andrew", "Angela",
        "Anna", "Anthony", "Aria", "Ariana", "Ashley", "Austin", "Ava", "Ben", "Benjamin", "Beth",
        "Blake", "Brandon", "Brian", "Brittany", "Brooke", "Caden", "Caleb", "Cameron", "Caroline", "Carter",
        "Charlotte", "Chloe", "Chris", "Christian", "Claire", "Connor", "Cooper", "Daniel", "David", "Delilah",
        "Dylan", "Ella", "Emily", "Emma", "Ethan", "Evelyn", "Gabriel", "Grace", "Hannah", "Harper",
        "Henry", "Hunter", "Isabella", "Isaac", "Jack", "Jackson", "Jacob", "James", "Jasmine", "Jayden",
        "Joseph", "Joshua", "Julia", "Julian", "Justin", "Kaitlyn", "Kayla", "Kevin", "Liam", "Lillian",
        "Lily", "Logan", "Lucas", "Luke", "Madeline", "Madison", "Mason", "Matthew", "Maya", "Michael",
        "Mila", "Nathan", "Natalie", "Nicholas", "Noah", "Nora", "Olivia", "Owen", "Parker", "Penelope",
        "Ryan", "Samantha", "Sarah", "Scarlett", "Sophia", "Sophie", "Stella", "Taylor", "Thomas", "Victoria",
        "William", "Zachary", "Zoe"
    };

    private static readonly Random Random = new Random();

    public static string GetRandomName()
    {
        if (Names.Count == 0)
            return string.Empty;

        int index = Random.Next(Names.Count);
        return Names[index];
    }
}