using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobbieWagnerGames.ProcGen
{
    public static class WaveFunctionCollapse
    {
        private static int CountUnsetCells(List<List<ProcGenCell>> grid)
        {
            return grid.SelectMany(row => row).Count(cell => cell.Value == -1);
        }

        public static async Task<List<List<ProcGenCell>>> CreateProceduralGridAsync(
            int width, 
            int height, 
            GenerationDetails details, 
            System.Random random = null)
        {
            return await Task.Run(() => CreateProceduralGrid(width, height, details, random));
        }

        public static List<List<ProcGenCell>> CreateProceduralGrid(
            int width,
            int height,
            GenerationDetails details,
            System.Random random = null)
        {
            random ??= details.Seed < 0 
                ? new System.Random() 
                : new System.Random(details.Seed);
            
            List<int> cellOptions = GenerateCellOptions(details);
            List<List<ProcGenCell>> grid = InitializeGrid(width, height, cellOptions);

            // Collapse initial random tile
            int firstX = random.Next(0, width);
            int firstY = random.Next(0, height);
            ProcGenCell firstCell = grid[firstY][firstX];

            CollapseCell(ref grid, details, firstCell, random);

            // Fill out the rest of the grid
            while (CountUnsetCells(grid) > 0 && !HasInvalidCells(grid))
            {
                CollapseNextCell(ref grid, details, random);
            }

            ValidateGrid(grid);
            return grid;
        }

        public static List<List<ProcGenCell>> InitializeGrid(int width, int height, List<int> cellOptions)
        {
            var grid = new List<List<ProcGenCell>>(height);
            
            for (int y = 0; y < height; y++)
            {
                grid.Add(new List<ProcGenCell>(width));
                for (int x = 0; x < width; x++)
                {
                    var cell = new ProcGenCell(x, y)
                    {
                        Options = new List<int>(cellOptions)
                    };
                    grid[y].Add(cell);
                }
            }

            return grid;
        }

        public static List<int> GenerateCellOptions(GenerationDetails details)
        {
            var cellOptions = new List<int>();
            
            for (int i = 0; i < details.Possibilities; i++)
            {
                int weight = details.Weights != null && details.Weights.TryGetValue(i, out weight) 
                    ? weight 
                    : 1;
                
                for (int addition = 0; addition < weight; addition++)
                {
                    cellOptions.Add(i);
                }
            }

            return cellOptions;
        }

        private static void CollapseNextCell(
            ref List<List<ProcGenCell>> grid,
            GenerationDetails details,
            System.Random random)
        {
            ProcGenCell nextCell = grid
                .SelectMany(row => row)
                .Where(cell => cell.Value == -1)
                .OrderBy(cell => cell.Options.Count)
                .First();

            CollapseCell(ref grid, details, nextCell, random);
        }

        public static void CollapseCell(
            ref List<List<ProcGenCell>> grid,
            GenerationDetails details,
            ProcGenCell cell,
            System.Random random)
        {
            int cellValue = cell.Options[random.Next(0, cell.Options.Count)];
            CollapseCell(ref grid, details, cell, random, cellValue);
        }

        public static void CollapseCell(
            ref List<List<ProcGenCell>> grid,
            GenerationDetails details,
            ProcGenCell cell,
            System.Random random,
            int cellValue)
        {
            cell.Value = cellValue;

            // Update adjacent tiles
            UpdateNeighborOptions(grid, details, cell, cellValue);
        }

        private static void UpdateNeighborOptions(
            List<List<ProcGenCell>> grid,
            GenerationDetails details,
            ProcGenCell cell,
            int cellValue)
        {
            if (cell.Y < grid.Count - 1)
            {
                ProcGenCell above = grid[cell.Y + 1][cell.X];
                above.Options = above.Options.Intersect(details.AboveAllowList[cellValue]).ToList();
            }

            if (cell.Y > 0)
            {
                ProcGenCell below = grid[cell.Y - 1][cell.X];
                below.Options = below.Options.Intersect(details.BelowAllowList[cellValue]).ToList();
            }

            if (cell.X > 0)
            {
                ProcGenCell left = grid[cell.Y][cell.X - 1];
                left.Options = left.Options.Intersect(details.LeftAllowList[cellValue]).ToList();
            }

            if (cell.X < grid[0].Count - 1)
            {
                ProcGenCell right = grid[cell.Y][cell.X + 1];
                right.Options = right.Options.Intersect(details.RightAllowList[cellValue]).ToList();
            }
        }

        private static bool HasInvalidCells(List<List<ProcGenCell>> grid)
        {
            return grid.SelectMany(row => row).Any(cell => cell.Options.Count <= 0);
        }

        private static void ValidateGrid(List<List<ProcGenCell>> grid)
        {
            if (HasInvalidCells(grid))
            {
                string gridState = string.Join("\n", 
                    grid.Select(row => 
                        string.Join(", ", row.Select(cell => cell.Value))));
                
                throw new InvalidOperationException(
                    $"Could not complete operation on grid: found no possible tiles to place at a cell\n{gridState}");
            }

            if (CountUnsetCells(grid) > 0)
            {
                throw new Exception("Failed to generate grid, please try again.");
            }
        }
    }
}