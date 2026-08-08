// Pocket Forge build pipeline.
//
// Kept in the repository so the pipeline is reviewed and versioned like the rest of
// the project instead of living in Jenkins' GUI configuration.
//
// Required Jenkins credentials (Manage Jenkins > Credentials):
//   pocketforge-keystore       Secret file   the upload keystore
//   pocketforge-keystore-pass  Secret text   keystore password
//   pocketforge-keyalias-pass  Secret text   key alias password
//
// Required node environment:
//   UNITY_EDITOR   full path to Unity.exe (for example
//                  C:\Program Files\Unity\Hub\Editor\6000.5.4f1\Editor\Unity.exe)

pipeline {
    agent any

    options {
        timestamps()
        timeout(time: 90, unit: 'MINUTES')
        buildDiscarder(logRotator(numToKeepStr: '30', artifactNumToKeepStr: '10'))
        disableConcurrentBuilds()
    }

    // Polling stands in for a webhook because this controller is not reachable from
    // GitHub. It only fetches refs; a build starts solely when main actually moved.
    triggers {
        pollSCM('H/10 * * * *')
    }

    parameters {
        booleanParam(
            name: 'BUILD_ANDROID',
            defaultValue: false,
            description: 'Also produce a signed release AAB. Tests always run.')
    }

    environment {
        UNITY = "${env.UNITY_EDITOR}"
        PROJECT_PATH = "${env.WORKSPACE}"
        ARTIFACTS = "${env.WORKSPACE}\\Builds\\ci"
        POCKETFORGE_VERSION_CODE = "${env.BUILD_NUMBER}"
    }

    stages {
        stage('Prepare') {
            steps {
                // Old AABs survive in the reused workspace and would be archived again
                // under the current build, so every stale output goes first.
                bat """
                    if not exist "%ARTIFACTS%" mkdir "%ARTIFACTS%"
                    if exist "%ARTIFACTS%\\editmode-results.xml" del "%ARTIFACTS%\\editmode-results.xml"
                    if exist "%ARTIFACTS%\\*.aab" del /q "%ARTIFACTS%\\*.aab"
                    if exist "%ARTIFACTS%\\size.csv" del "%ARTIFACTS%\\size.csv"
                """
                script {
                    if (!env.UNITY_EDITOR?.trim()) {
                        error('UNITY_EDITOR is not set on this node; the pipeline cannot locate Unity.')
                    }
                }
            }
        }

        // Compilation is its own stage so a broken script is reported as a compile
        // failure rather than surfacing later as a confusing test error.
        stage('Compile') {
            steps {
                bat """
                    "%UNITY%" -batchmode -quit -nographics ^
                        -projectPath "%PROJECT_PATH%" ^
                        -executeMethod PocketForge.EditorTools.PocketForgeCi.AssertScriptsCompiled ^
                        -logFile "%ARTIFACTS%\\compile.log"
                """
            }
            post {
                failure {
                    archiveArtifacts artifacts: 'Builds/ci/compile.log', allowEmptyArchive: true
                }
            }
        }

        stage('EditMode tests') {
            steps {
                // -runTests returns a non-zero exit code when tests fail, and the
                // Compile stage above already proved the test assembly is current, so a
                // green result here cannot come from a stale assembly.
                bat """
                    "%UNITY%" -batchmode -nographics ^
                        -projectPath "%PROJECT_PATH%" ^
                        -runTests -testPlatform EditMode ^
                        -testResults "%ARTIFACTS%\\editmode-results.xml" ^
                        -logFile "%ARTIFACTS%\\tests.log"
                """
            }
            post {
                always {
                    nunit testResultsPattern: 'Builds/ci/editmode-results.xml', failIfNoResults: true
                    archiveArtifacts artifacts: 'Builds/ci/tests.log', allowEmptyArchive: true
                }
            }
        }

        stage('Signed AAB') {
            when {
                anyOf {
                    expression { return params.BUILD_ANDROID }
                    branch 'main'
                }
            }
            steps {
                withCredentials([
                    file(credentialsId: 'pocketforge-keystore', variable: 'POCKETFORGE_KEYSTORE_PATH'),
                    string(credentialsId: 'pocketforge-keystore-pass', variable: 'POCKETFORGE_KEYSTORE_PASS'),
                    string(credentialsId: 'pocketforge-keyalias-pass', variable: 'POCKETFORGE_KEYALIAS_PASS')
                ]) {
                    bat """
                        set POCKETFORGE_ANDROID_OUTPUT=%ARTIFACTS%\\PocketForge-%BUILD_NUMBER%.aab
                        "%UNITY%" -batchmode -quit -nographics ^
                            -projectPath "%PROJECT_PATH%" ^
                            -executeMethod PocketForge.EditorTools.PocketForgeAndroidBuild.BuildReleaseAab ^
                            -logFile "%ARTIFACTS%\\build.log"
                    """
                }
            }
            post {
                always {
                    archiveArtifacts artifacts: 'Builds/ci/build.log', allowEmptyArchive: true
                }
            }
        }

        stage('Report size') {
            when {
                anyOf {
                    expression { return params.BUILD_ANDROID }
                    branch 'main'
                }
            }
            steps {
                script {
                    def aab = "Builds/ci/PocketForge-${env.BUILD_NUMBER}.aab"
                    if (!fileExists(aab)) {
                        error("Expected the AAB at ${aab} but the build stage produced nothing.")
                    }

                    // Reading the size through a bat + readFile keeps this inside the
                    // Groovy sandbox; new File() on the controller is rejected.
                    bat """
                        for %%I in ("${aab.replace('/', '\\\\')}") do @echo %%~zI> Builds\\ci\\size.txt
                    """
                    def bytes = readFile('Builds/ci/size.txt').trim() as Long
                    def mib = bytes / 1048576.0
                    // The download budget the project committed to; exceeding it is a
                    // warning rather than a failure so the artifact is still inspectable.
                    if (mib > 50) {
                        unstable("AAB is ${String.format('%.2f', mib)} MiB, over the 50 MiB budget.")
                    }
                    echo "AAB size: ${String.format('%.2f', mib)} MiB"
                    // Header + single row per build; the Plot plugin reads this format
                    // and renders the size trend across builds.
                    writeFile(
                        file: 'Builds/ci/size.csv',
                        text: "AAB MiB\n${String.format('%.2f', mib)}\n")
                }
                plot(
                    csvFileName: 'plot-aab-size.csv',
                    group: 'Build output',
                    title: 'AAB size (MiB)',
                    style: 'line',
                    numBuilds: '30',
                    yaxis: 'MiB',
                    csvSeries: [[file: 'Builds/ci/size.csv', inclusionFlag: 'OFF', displayTableFlag: false]])
                archiveArtifacts artifacts: 'Builds/ci/*.aab,Builds/ci/size.csv', fingerprint: true
            }
        }
    }

    post {
        failure {
            echo "Pipeline failed at stage: ${env.STAGE_NAME}"
        }
        cleanup {
            bat 'if exist "%ARTIFACTS%\\build.log" del "%ARTIFACTS%\\build.log"'
        }
    }
}
