using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace PlayingWithLibGit2
{
    public static class Program
    {
        static void Main(string[] args)
        {
            // Create the repo
            var rootedPath = Repository.Init("../../Repo", false);
            Console.WriteLine("repo: {0}", rootedPath);

            using (var repo = new Repository("../../Repo"))
            {
                // create the file
                const string content = "Commit this!\nAnd do something else";
                File.WriteAllText(Path.Combine(repo.Info.WorkingDirectory, "fileToCommit.txt"), content);

                // stage the file
                repo.Stage("fileToCommit.txt");

                // Create the committer's signature and commit
                var author = new Signature("Jose Jones", "josej@medamerica.com", DateTime.Now);
                var committer = author;

                // Commit to the repo
                var commit = repo.Commit("Here's a commit I made!", author, committer);
                foreach (var parent in commit.Parents)
                {
                    Console.WriteLine("Id: {0}, Sha: {1}", parent.Id, parent.Sha);
                }
            }
        }
    }
}
