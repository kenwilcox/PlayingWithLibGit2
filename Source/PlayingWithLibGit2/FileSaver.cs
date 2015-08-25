using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace PlayingWithLibGit2
{
    /// <summary>
    /// Saves a file and puts it in a git repository
    /// </summary>
    public class FileSaver
    {
        private readonly string _basePath;
        private readonly string _repoPath;

        public string BasePath
        {
            get { return _basePath;}
        }

        public string RepoPath
        {
            get { return _repoPath;}
        }

        public FileSaver(string basePath)
        {
            _basePath = basePath;
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            _repoPath = Repository.Init(_basePath);
        }

        public void SaveFile(string fileName, string content, string username, string email)
        {
            using (var repo = new Repository(_basePath))
            {
                File.WriteAllText(Path.Combine(repo.Info.WorkingDirectory, fileName), content);

                // stage the file
                repo.Stage(fileName);

                // Create the committer's signature and commit
                //var user = repo.Config.Get<string>("user", "name", null);
                //var email = repo.Config.Get<string>("user", "email", null);

                var author = new Signature(username, email, DateTime.Now);
                var committer = author;

                // Commit to the repo
                var commitMessage = string.Format("Revision: {0}", GetRevisionCount(repo, fileName));
                try
                {
                    var commit = repo.Commit(commitMessage, author, committer);
                    foreach (var parent in commit.Parents)
                    {
                        Console.WriteLine("Id: {0}, Sha: {1}", parent.Id, parent.Sha);
                    }
                }
                catch (EmptyCommitException) { } // I don't care if the user didn't change anything at this time
            }
        }

        public List<string> GetCommitHistory()
        {
            using (var repo = new Repository(_basePath))
            {
                // Get the revisions in reverse order
                // git log --topo-order --reverse
                var filter = new CommitFilter { SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Reverse };

                return repo.Commits.QueryBy(filter).Select(c => string.Format("{0} {1}<{2}> - {3}", c.Sha.Substring(0, 7), c.Author.Name, c.Author.Email, c.MessageShort)).ToList();
            }
        } 

        private static int GetRevisionCount(IRepository repo, string fileName)
        {
            var modificationCommits = new List<Commit>();
            var currentSha = string.Empty;
            var currentPath = fileName;
            Commit temp = null;

            foreach (var c in repo.Commits)
            {
                if (c.Tree.Any(entry => entry.Name == currentPath))
                {
                    // If file with given name was found, check its SHA
                    var te = c.Tree.First(entry => entry.Name == currentPath);
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
                    var te = c.Tree.First(entry => entry.Target.Sha == currentSha);
                    currentSha = te.Target.Sha;
                    currentPath = te.Name;

                    modificationCommits.Add(temp);
                }
            }

            if (null != temp)
            {
                modificationCommits.Add(temp);
            }


            return modificationCommits.Count + 1;
        }
    }
}
