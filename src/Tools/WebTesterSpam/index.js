const argv = require('minimist')(process.argv.slice(2));
const chalk = require("chalk")

const options = Object.assign({
    w: 5,
    i: 10,
    q: '6158dd074d64498f8a50e5e9828fda23', // enumeration question automation
    u: 'andrii',
    p: '1',
    h: false,
    address: "https://localhost:5002"
}, argv)

const puppeteer = require('puppeteer');
const { Cluster } = require('puppeteer-cluster');

const config = {
    launchOptions: {
        ignoreHTTPSErrors: true,
        headless: !options.h,
        defaultViewport: { width: 1920, height: 1080 }
    },
    viewport: { width: 1920, height: 1080 }
}

async function testWeb({ page }) {
    await page.goto(options.address, { waitUntil: 'networkidle2' });
    await page.type('#Input_Email', options.u)
    await page.type('#Input_Password', options.p)
    await page.click('[type="submit"]')
    await page.waitForSelector("#questionnaire-table-header")

    var iterationDone = new Promise(async function(resolve) {
        page.on("popup", async target => {
            const tester = target
    
            tester.waitForNavigation({
                timeout: 20000
            })
    
            console.info(`tester : ${tester}`)
            await tester.waitForSelector("#loadingPixel", {
                timeout: 20000
            })
    
            var waitLoading = async () => {
                await tester.waitForFunction(() => {
                    try {
                        const result = document.getElementById('loadingPixel').getAttribute('data-loading') === 'false'
                        return result
                    }
                    catch (e) {
                        return false;
                    }
                },
                {
                    timeout: 50000,
                    polling: 1000
                })
            }
    
            await waitLoading()
            await tester.evaluate(() => window._api.router.push({ name: 'complete' }))
            await waitLoading()
            await tester.waitForSelector('#btnComplete')
            await tester.click('#btnComplete')
            resolve()
        })
    
        await page.goto(options.address + "/questionnaire/details/" + options.q)
        await page.waitForSelector('#webtest-btn')
        await page.click('#webtest-btn')
    });
    return iterationDone
}


(async () => {

    const cluster = await Cluster.launch({
        concurrency: Cluster.CONCURRENCY_CONTEXT,
        maxConcurrency: options.w,
        puppeteerOptions: config.launchOptions
    });

    await cluster.task(testWeb);

    for (let i = 1; i <= options.i; i++) {
        cluster.queue(i);
    };
    console.info('all tasks are in queue')

    await cluster.idle();
    await cluster.close();
})();