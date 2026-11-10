# Trusted Minesweeper validation suite

This test project is the trusted test suite for the CODE RED Minesweeper candidate workflow.

The candidate-validation workflow checks out this test project from the trusted `development` ref, checks out candidate source into a separate directory, and copies only the candidate's `src/Minesweeper` project into the trusted workspace before running these tests.

The project reference is intentionally resolved only after the workflow stages candidate source at `src/Minesweeper`. This project is not included in the normal solution/build matrix; it is executed by the dedicated `test-minesweeper` workflow job.

Do not edit these tests from the candidate branch. Changes to the trusted test suite require their own reviewed change on `development`. The candidate can change the implementation under test, not the assertions used to judge it.
