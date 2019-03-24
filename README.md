# CodeInsight

CodeInsight is Version Control System analysis tools, which opposed to standard static analysis consideres time another factor. It does not only see the result, but also how you got to it. The idea is very similar to https://codescene.io/.
Simple example would be that static analysis would show you code areas to refactor, but with the time added, CodeInsight may tell you, that there is no need for that, as you haven't touched that part of code for a long time and there is a lesser need to improve it than some other frequently touched pieces of your codebase.

As for now, CodeInsight provides integration with Github and analyses only Pull Requests.
The flow is simple, just login via your github account and grant access to organizations you want.
You will be offered to select one of your repositories to analyze and after a while of importing data, you are taken directly to you statistics.
