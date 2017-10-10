<html>
<body>

<h1>GitHub API | Code Challenge<br></h1> 
<p><u>The goal of this exercise is to write an HTTP service with the following specs:</u>
Given a <b>city name</b> (<i>e.g. Barcelona</i>) the service returns a list of the <b>top contributors</b> (number of commits). Results sorted by number of repositories in GitHub.
The service should give the possibility to choose between the Top 50, Top 100 or Top
150 contributors</p>

<h3>GitHub API Limits (using personal API tokens)</h3>
<ul>
    <li>30 requests minute</li>
    <li>1.000 total results for search (with the same filters paged)</li>
    <li>5000 requests hour</li>
    <li>100 results per page</li>
</ul>

<hr>

<h3>Solution</h3>

<b>Technologies</b>

<ul>
    <li>REST Http Service with <b>.NET Core 2</b><br>Download SDK: <a href="https://www.microsoft.com/net/download/core">https://www.microsoft.com/net/download/core</a></li>
    <li>C# 7 with Visual Studio 2017</li>
    <li><a href="https://github.com/octokit/octokit.net">Octokit.GitHub</a></li>
</ul>

You could download <a href="https://code.visualstudio.com/">Visual Studio Code</a> with <a href="https://github.com/OmniSharp/omnisharp-vscode">OmniSharp extension</a> for support for <b>C# 7.1 & .NET Core 2</b>.

<br>
I have used gitHub personal API token: <br>
<i>Please you must generate valid token for you <a href="https://github.com/blog/1509-personal-api-tokens">here</a> and paste it in <b><i>Service/Impl/OctokitGitHubApiProvider.cs</i></b></i>

<h3>API method (endpoints examples)</h3>

<pre>
    api/user/<b>Valencia</b>/<i>50</i><br>
    api/user/<b>Barcelona</b>/<i>100</i><br>
    api/user/<b>Tarragona</b>/<i>150</i><br>
</pre>    

<h3>Response</h3>

 <ul>
    <li> 
        <B>Status</B>
        <ul>
            <li><U>FINISHED</U>: The process is done.</li>
            <li><u>RUNNING</u>: The process is runnning (getting users by location).</li>
            <li><u>CALCULATING_ORDER</u>: The process is calculating order (getting total of commits and repositories by user).</li>
            <li><u>STOPPED</u>: The process not have loaded this location.</li>
        </ul>
    </li>
    <li><B>TotalResults</B>: Total results found</li>
    <li><B>TotalResultsLoaded</B>: Number of users loaded processed</li>
    <li><B>LastUpdated</B> <I>[Only when is FINISHED]</I>: Date time with last results updated</li>
    <li><B>TotalOrderCalculated</B> <I>[Only when is CALCULATING_ORDER]</I>: Number of users with number of commits and repositories processsed fetched</li>
    <li><B>Users</B>: List of top 50/100/150 contributors</li>
 </ul>

<h3>Cache & Repository </h3>
<p>The first MVP use process caching for store all necessary information and sort list in memory. All code is decouple with interfaces (interface segregation principle). Now is very easly implement any other distributed caching technology like Redis (Open/Close principle).</p>
<img src="https://github.com/josecuellar/github-ranking-contributors-city/blob/master/src/GitHub.API/Images/implmemory.jpg">

<h3>Next steps</h3>
 <ul>
    <li>Implement <a href="https://redis.io/">Redis</a> repository</li>
    <li>Use <a href="https://redis.io/commands/sort">SortedList</a> rather than Linq</li>
    <li>Optimize performance using tools like <a href="https://github.com/dotnet/BenchmarkDotNet">BenchmarkDotNet</a></li>
    <li>Deploy solution in <a href="https://www.docker.com/what-container">Docker Container</a></li>
 </ul>

<h4>SNAPSHOT Running Status</h4>
<img src="https://github.com/josecuellar/github-ranking-contributors-city/blob/master/src/GitHub.API/Images/running2.jpg">

<h4>SNAPSHOT Calculating Order Status</h4>
<img src="https://github.com/josecuellar/github-ranking-contributors-city/blob/master/src/GitHub.API/Images/calculatingorder.jpg">

<h4>SNAPSHOT Finished Status</h4>
<img src="https://github.com/josecuellar/github-ranking-contributors-city/blob/master/src/GitHub.API/Images/finished.jpg">

</body>
</html>



