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
                const string content = @"Some Header Information

  var i = 0;
  var j = 10;
  var k = i + j;
";
                File.WriteAllText(Path.Combine(repo.Info.WorkingDirectory, "Q5020.txt"), content);

                // stage the file
                repo.Stage("Q5020.txt");

                // Create the committer's signature and commit
                var author = new Signature("Jose Jones", "josej@medamerica.com", DateTime.Now);
                var committer = author;

                // Commit to the repo
                var commitMessage = string.Format("Revision: {0}", repo.Commits.Count() + 1);
                try
                {
                    var commit = repo.Commit(commitMessage, author, committer);
                    foreach (var parent in commit.Parents)
                    {
                        Console.WriteLine("Id: {0}, Sha: {1}", parent.Id, parent.Sha);
                    }
                }
                catch (EmptyCommitException) { } // I don't care if the user didn't change anything at this time

                // Get the revisions in reverse order
                // git log --topo-order --reverse
                var filter = new CommitFilter { SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Reverse };

                foreach (var c in repo.Commits.QueryBy(filter))
                {
                    Console.WriteLine("{0} {1} - {2}", c.Sha.Substring(0, 7), c.Author.Name, c.MessageShort);
                }
            }
        }
    }
}
