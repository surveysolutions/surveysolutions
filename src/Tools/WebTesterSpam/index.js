const argv = require('minimist')(process.argv.slice(2));
const chalk = require("chalk")

const options = Object.assign({
    w: 5,
    i: 10,
    q: '6158dd074d64498f8a50e5e9828fda23', // enumeration question automation
    u: 'admin',
    p: '1',
    h: false,
    address: "https://localhost/designer"
}, argv)

const puppeteer = require('puppeteer');

(async () => {
    const browser = await puppeteer.launch({
        ignoreHTTPSErrors: true,
        headless: !options.h,
    });

    const designer = await browser.newPage();

    await designer.setViewport({ width: 1600, height: 900 })

    await designer.goto(options.address, { waitUntil: 'networkidle2' });
    await designer.type('#UserName', options.u)
    await designer.type('#Password', options.p)
    await designer.click('[type="submit"]')

    console.log(chalk.green("Logged in"))
    await designer.waitForSelector("#questionnaire-table-header")

    console.log("Loading questionnaire")
    await designer.goto(options.address + "/questionnaire/details/" + options.q)
    await designer.waitForSelector('#webtest-btn')
    
    const work = {
        inWork: 0,
        left: options.i
    }

    console.log(`Starting work. ${work.left} interviews left`)

    browser.on("targetcreated", async target => {
        try {
            const tester = await target.page()
            work.inWork += 1

            tester.waitForNavigation({
                timeout: 90000
            })
            await tester.setViewport({ width: 1600, height: 900 })
            await tester.waitForSelector("[data-id=SidebarCompleted]", {
                timeout: 90000
            })
            await tester.evaluate("window._api.router.push({name: 'complete'})")
            await tester.evaluate('window._api.store.dispatch("completeInterview", { comment: "test"})')          
            
        } catch (e) {
            console.log(chalk.red("Error in interview"), e)
        } finally{
            work.inWork -= 1
            work.left--
        }

        console.log("Completed interview. " + work.left + ' left')

        if (work.left - work.inWork > 0) {
            await designer.click('#webtest-btn')
        } else {
            if (work.inWork <= 0) {
                await browser.close();
            }
        }
    })

    for (var i = 0; i < Math.min(options.w, work.left); i++) {
        await designer.click('#webtest-btn')
    }

})();