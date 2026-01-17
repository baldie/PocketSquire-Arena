const { execFileSync, execSync } = require('child_process');

const isWin = process.platform === 'win32';
const npmCmd = isWin ? 'npm.cmd' : 'npm';
const bdCmd = isWin ? 'bd.exe' : 'bd'; // Assuming bd is an executable; if it's a script, might need .cmd/.ps1

function run(command, args = []) {
    console.log(`Running: ${command} ${args.join(' ')}`);
    try {
        return execFileSync(command, args, {
            encoding: 'utf8',
            stdio: 'inherit',
            shell: isWin // Use shell on windows for better command resolution
        });
    } catch (error) {
        console.error(`Command failed: ${command}`);
        process.exit(1);
    }
}

function capture(command, args = []) {
    try {
        return execFileSync(command, args, {
            encoding: 'utf8',
            shell: isWin
        }).trim();
    } catch (error) {
        console.error(`Failed to capture output: ${command}`);
        process.exit(1);
    }
}

// 1. Run Quality Gates
console.log('--- Running Quality Gates ---');
run(npmCmd, ['run', 'test:unit']);
// run(npmCmd, ['run', 'test:integration']);

// 2. Get Current Bead ID
const beadId = capture(bdCmd, ['config', 'get', 'current_bead']);
if (!beadId) {
    console.error('No active bead found in config.');
    process.exit(1);
}

// 3. Close Bead
console.log(`--- Closing Bead: ${beadId} ---`);
run(bdCmd, ['close', beadId]);
run(bdCmd, ['config', 'unset', 'current_bead']);
run(bdCmd, ['sync']);

// 4. Push Branch
console.log('--- Pushing Changes ---');
// Push current branch to origin
run('git', ['push', '--set-upstream', 'origin', 'HEAD']);

console.log('\n--- Creating Pull Request ---');
try {
    console.log('Running: gh pr create --fill');
    execFileSync('gh', ['pr', 'create', '--fill'], {
        encoding: 'utf8',
        stdio: 'inherit',
        shell: isWin
    });
    console.log('\n✅ Task submitted & PR created successfully!');
} catch (e) {
    console.warn('\n⚠️  Task submitted, but PR creation failed (is `gh` installed and authenticated?).');
    console.warn('Please create the PR manually.');
}
