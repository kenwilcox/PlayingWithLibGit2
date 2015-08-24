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
            var fileName = "fileToCommit.txt";

            // Create the repo
            var rootedPath = Repository.Init("../../Repo", false);
            Console.WriteLine("repo: {0}", rootedPath);

            using (var repo = new Repository("../../Repo"))
            {
                // create the file
                const string content = @"Commit this!
And do something else
And, what the heck another line
And, another
";
                File.WriteAllText(Path.Combine(repo.Info.WorkingDirectory, fileName), content);

                // stage the file
                repo.Stage(fileName);

                // Create the committer's signature and commit
                var author = new Signature("Jose Jones", "josej@medamerica.com", DateTime.Now);
                var committer = author;

                // Commit to the repo
                var commitMessage = string.Format("Revision: {0}", GetRevisionCount(repo, fileName));//repo.Commits.Count() + 1);                
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

        private static int GetRevisionCount(Repository repo, string fileName)
        {
            var modificationCommits = new List<Commit>();
            var currentSha = string.Empty;
            var currentPath = fileName;
            Commit temp = null;

            foreach (var c in repo.Commits)
            {
                if (c.Tree.Any<TreeEntry>(entry => entry.Name == currentPath))
                {
                    // If file with given name was found, check its SHA
                    var te = c.Tree.First<TreeEntry>(entry => entry.Name == currentPath);
                    if (te.Target.Sha == currentSha)
                    {
                        // In case if file's SHA matches file was not changed in this commit 
                        // and temporary commit need to be updated to current one
                        temp = c;
                    }
                    else
                    {
                        // In case if file's SHA don't match file was changed during commit
                        // and temporary commit need to be added to the commits collection 
                        // as the one where update occur. The file's SHA updated to current one
                        modificationCommits.Add(temp);
                        currentSha = te.Target.Sha;
                    }
                }
                else
                {
                    // File with given name not found. this means it was renamed. 
                    // However ut's SHA still the same, so it can be found by it.
                    if (c.Tree.All(entry => entry.Target.Sha != currentSha)) continue;
                    var te = c.Tree.First<TreeEntry>(entry => entry.Target.Sha == currentSha);
                    currentSha = te.Target.Sha;
                    currentPath = te.Name;

                    modificationCommits.Add(temp);
                }
            }

            if (null != temp)
            {
                modificationCommits.Add(temp);
            }


            return modificationCommits.Count;
        }
    }
}
