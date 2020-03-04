# Introduction
Use Unity 2019.3 for editing.

## Cloning
Please make sure to have your login information available when you clone the repository. It will ask you to login.

If you use the git CLI, please set up an SSH key following the instructions at the following link:
https://help.github.com/en/github/authenticating-to-github/generating-a-new-ssh-key-and-adding-it-to-the-ssh-agent. If you use Windows, you'll need git bash to do this.

## GitHub Large File Storage
GitHub's LFS is a way to store image files, 3D models, and other large files without placing them inside the repository. Please install GitHub LFS from https://git-lfs.github.com/ and follow Step 1 of the Getting Started section. This will prepare the repository for use with the LFS API.

If something seems to be missing or working strangely, attempt pulling the actual object from the GitHub LFS server using `git lfs pull`.

## Opening the Project for the First Time
Tiff has commented instructions for correctly setting up the project in fiscal-shock/fiscal-shock.csproj. Please read those instructions closely and follow them the next time you set up your project.

## WARNINGS
- Meta files get added for assets, scripts, *and* directories (folders). Please do not name two files or directories the same. If we end up with fragmented directories, we can always consolidate them later.


# Rules for Contribution
The following are the rules of contribution. Please follow them, as they will make both your life and our lives easier.

## Branches
Push to master has been disabled, so you should not be able to push your code directly to the master branch, even via force push. All new code should be written inside of a branch. While it is not necessary, it would be helpful if you prefaced branch names with "feature/" or "bugfix/". 

Unless you are helping another team member, don't work on the same branch as someone else. If you do work on the same branch as someone, make sure you have their permission and the latest code from that branch, preferably by asking the other team member to push any finished code to the remote repository.

## Formatting
- Blocks of code should be formatted so that each nested block is tabbed forward another set of spaces.
- For tabbing, use four *spaces*. This can be edited in your preferred text editor.
- The following naming conventions apply:
    - Use PascalCase for classes.
    - Use uppercase SNAKE_CASE for constant variables. This does not mean literal constants (i.e. JavaScript's "const" keyword), but functional constants (i.e. variables whose values determine magic numbers, file names, and other such unchanging values). If the value of the variable or any values in an object change, it is not truly a constant.
    - Use camelCase for any functions, variables, or other identifiers that do not fall into the previous two categories.

## Editing
Feel free to use any text editor or IDE that you like, but please look up the gitignore file for your preferred editor. You can find different gitignore setups for different editors at the following link: https://gitignore.io/

## Issues
If there is something that needs to be done related to the code in the repository, please create an issue in the GitHub repository. If an issue is not automatically assigned to the project's To Do list, please add it in there, as it will allow us to give some priority to the issue. Issues in general should be formatted in the following way:
- Epics are overarching issues that consist of many subtasks. Create these issues first, assign them a milestone, and add a checklist in the description of tasks that need to be completed to finish the epic.
- Tasks are smaller, "bite-sized" issues that can be handled by one person. These issues should stem off of the epics. Once you've created all of the relevant tasks, make sure to go back to the epic and, using the #ISSUE_NUM syntax, add a link to each checklist item to the relevant issue for the task.

Please check off issues from epics as those issues are closed. Some tips to making your life easier by automating the project tracking process include:
- You can reference a GitHub issue in a pull request by adding #ISSUE_NUM somewhere in the request. This will link it automatically to the issue associated with ISSUE_NUM.
- By using "closes" in the description of your pull request alongside the issue number reference, the issue will automatically close itself after your pull request has been approved and your branch merged into master.

## Pull Requests
Pull requests are required for all merges to master. At least two reviewer approvals are required in order to fully prepare the branch for merging. Please add functional validation tests so that we know what specifically we are trying to test when we are reviewing PRs and ensuring that they won't break something else. This can be done by commenting on your pull request a checklist of items that we should check for when we are reviewing the code.

If at all possible, please try to use the tips above to automate pull requests and issue handling. It will help us later on when we have several dozen issues to handle. 

## Other Git Tips
- Make sure you have the latest code by pulling using `git pull`.
- You can temporarily save work without committing using `git stash`.
    - `git stash list` enumerates your current stash
    - `git stash show` shows a difference of the stashed changeset
    - `git stash pop` pops off the most recently stashed changeset; will optionally take a stack number to bypass stack behavior
    - `git stash clear` will clear all stashed changesets
    - `git stash apply` applies the most recently stashed changeset without clearing it from the stash
    - More information on using the stash can be found at https://git-scm.com/docs/git-stash.
- Be careful with merge conflicts. If you have no idea how to handle them, ask for help. There's a lot of mistakes that can be made if someone isn't careful with conflict remediation.
- If you need to work from features in another person's branch, branch off their branch by switching to their branch using `git checkout BRANCH_NAME` and running `git checkout -b NEW_BRANCH_NAME`, which will create a new branch with the given name based off the branch you were just in. As stated above, don't work in another person's branch if it isn't necessary.
    - If you accidentally commit to master, you can branch off of those changes and reset master using `git reset origin --hard`. Try not to commit to master.
- `git cherry-pick [revision hash]` lets you steal a commit from another branch if you accidentally commit to the wrong branch, but you'll still need to clean up any merge conflicts or other situations that occur.
- Git is very powerful. Dr. Siy was not joking with that picture of the multi-use gun from the first day of class. Therefore, don't be afraid to ask questions from those more experienced. Also, the following link contains some helpful beginner information for git: https://rogerdudler.github.io/git-guide/.