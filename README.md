<html>
<body>

<h1>GitHub API | Code Challenge<br></h1> 
<p><u>The goal of this exercise is to write an HTTP service with the following specs:</u>
Given a <b>city name</b> (<i>e.g. Barcelona</i>) the service returns a list of the <b>top contributors</b> (number of commits). Results sorted by number of repositories in GitHub.
The service should give the possibility to choose between the Top 50, Top 100 or Top
150 contributors</p>

<h4>GitHub API Rate Limit</h4>
<ul>
    <li>30 requests minute for authorized user</li>
    <li>1.000 total results for search (with the same filters paged)</li>
    <li>5000 requests hour</li>
    <li>100 results per page</li>
</ul>

<hr>

<h4>Solution</h4>

<b>Technologies</b>

<ul>
    <li>REST Http Service with <b>.NET Core 2</b><br>Download SDK: <a href="https://www.microsoft.com/net/download/core">https://www.microsoft.com/net/download/core</a></li>
    <li>C# 7 with Visual Studio 2017</li>
    <li><a href="https://github.com/octokit/octokit.net">Octokit.GitHub</a></li>
</ul>

You can download <a href="https://code.visualstudio.com/">Visual Studio Code</a> with <a href="https://github.com/OmniSharp/omnisharp-vscode">OmniSharp extension</a> for support for <b>C# 7.1 & .NET Core 2</b>.

<b>Using API method (valid endpoints)</b>

<p>
    api/user/<b>Valencia</b>/<i>50</i><br>
    api/user/<b>Barcelona</b>/<i>100</i><br>
    api/user/<b>Tarragona</b>/<i>150</i><br>
</p>    

<h4>Response</h4>

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

<u>SNAPSHOT Running Status</u>
![alt text](http://github-ranking-contributors-city/src/GitHub.API/Images/running.jpg)


</body>
</html>



