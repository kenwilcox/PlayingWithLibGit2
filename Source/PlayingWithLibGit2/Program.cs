using System;

namespace PlayingWithLibGit2
{
    public static class Program
    {
        static void Main()
        {
            const string fileName = "anotherFile.txt";
            const string content = "This is just another file!\n";
            var fs = new FileSaver("../../Repo");
            Console.WriteLine("BasePath: {0}", fs.BasePath);
            Console.WriteLine("RepoPath: {0}", fs.RepoPath);

            fs.SaveFile(fileName, content, "Jose Jones", "email");

            foreach (var line in fs.GetCommitHistory())
            {
                Console.WriteLine(line);
            }
        }
    }
}
