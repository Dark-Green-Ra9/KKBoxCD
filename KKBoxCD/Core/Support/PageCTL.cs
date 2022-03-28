using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace KKBoxCD.Core.Support
{
    public class PageCTL
    {
        private readonly Page mPage;

        public delegate bool WaitForUrlHandler(string url);

        public PageCTL(Page page)
        {
            mPage = page;
        }

        public async Task<bool> GoToAsync(string url, string selector = null, NavigationOptions wait = null, int times = 3)
        {
            while (times > 0)
            {
                try
                {
                    await mPage.GoToAsync(url, wait);
                    if (selector != null)
                    {
                        await mPage.WaitForSelectorAsync(selector);
                    }
                    return true;
                }
                catch { }
                times--;
            }
            return false;
        }

        public async Task<bool> TypeAsync(string text, string selector, TypeOptions options = null, int times = 3)
        {
            while (times > 0)
            {
                try
                {
                    await SetInputValueAsync(selector, string.Empty);
                    await Task.Delay(500);
                    await mPage.TypeAsync(selector, text, options);
                    string value = await GetInputValueAsync(selector);
                    return text.Equals(value, StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    await Task.Delay(500);
                }
                times--;
            }
            return false;
        }

        public async Task<bool> ExistAsync(string selector)
        {
            try
            {
                return await mPage.QuerySelectorAsync(selector) != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> WaitForExistAsync(string selector, int timeout = 30000)
        {
            do
            {
                if (await ExistAsync(selector))
                {
                    return true;
                }
                await Task.Delay(3000);
                timeout -= 3000;
            } while (timeout > 0);

            return false;
        }

        public async Task<bool> WaitForExistAnyAsync(string[] selectors, int timeout = 30000)
        {
            do
            {
                foreach (string selector in selectors)
                {
                    if (await ExistAsync(selector))
                    {
                        return true;
                    }
                }
                await Task.Delay(1000);
                timeout -= 1000;
            } while (timeout > 0);

            return false;
        }

        public async Task<bool> WaitForGoneAsync(string selector, int timeout = 30000)
        {
            do
            {
                if (!await ExistAsync(selector))
                {
                    return true;
                }
                await Task.Delay(3000);
                timeout -= 3000;
            } while (timeout > 0);

            return false;
        }

        public async Task<bool> WaitForUrl(Func<string, bool> func, int timeout = 30000)
        {
            do
            {
                if (func(mPage.Url))
                {
                    return true;
                }
                await Task.Delay(3000);
                timeout -= 3000;
            } while (timeout > 0);

            return false;
        }

        public async Task<bool> WaitForPropertyAsync(string selector, string property, object value, int timeout = 30000)
        {
            do
            {
                bool success = await mPage.EvaluateFunctionAsync<bool>(@"
                (selector, property, value) => {
                    const ele = document.querySelector(selector);
                    if (ele == null) return false;
                    return ele[property] == value;
                }", selector, property, value);
                if (success)
                {
                    return true;
                }
                await Task.Delay(3000);
                timeout -= 3000;
            }
            while (timeout > 0);

            return false;
        }

        public async Task<bool> WaitForElementAsync(string selector, Func<ElementHandle, bool> func, int timeout = 30000)
        {
            do
            {
                ElementHandle element;
                try
                {
                    element = await mPage.QuerySelectorAsync(selector);
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    element = null;
                }

                if (element != null && func(element))
                {
                    return true;
                }
                await Task.Delay(1000);
                timeout -= 1000;

            } while (timeout > 0);

            return false;
        }

        public async Task<bool> SetInputValueAsync(string selector, string value)
        {
            try
            {
                await mPage.EvaluateFunctionAsync(@"
                (selector, value) => {
                    const ele = document.querySelector(selector);
                    if (ele != null) ele.value = value;
                }", selector, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetInputValueAsync(string selector)
        {
            try
            {
                return await mPage.EvaluateFunctionAsync<string>(@"
                (selector) => {
                    const ele = document.querySelector(selector);
                    if (ele != null)
                    {
                        return ele.value;
                    }
                    else
                    {
                        return null;
                    }
                }", selector);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SetElementsIDAsync(string selector, string name, int start_index = 0)
        {
            try
            {
                await mPage.EvaluateFunctionAsync(@"
                (selector, name, start_index) => {
                    const eles = document.querySelectorAll(selector);
                    for(var i = 0; i < eles.length; i++) {
                        const ele = eles[i];
                        ele.id = name.concat(start_index + i);
                    }
                }", selector, name, start_index);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetChildsIDAsync(string selector, string name, int start_index = 0)
        {
            try
            {
                return await mPage.EvaluateFunctionAsync<bool>(@"
                (selector, name, start_index) => {
                    const parent = document.querySelector(selector);
                    if (parent == null) return false;
                    const eles = parent.children;
                    for(var i = 0; i < eles.length; i++) {
                        const ele = eles[i];
                        ele.id = name.concat(i);
                    }
                    return true;
                }", selector, name, start_index);
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetElementInnerHTML(string selector)
        {
            try
            {
                ElementHandle element = await mPage.QuerySelectorAsync(selector);
                JSHandle handler = await element.GetPropertyAsync("innerHTML");
                return handler.ToString().Replace("JSHandle:", string.Empty);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GetElementInnerText(string selector)
        {
            try
            {
                ElementHandle element = await mPage.QuerySelectorAsync(selector);
                JSHandle handler = await element.GetPropertyAsync("innerText");
                return handler.ToString().Replace("JSHandle:", string.Empty);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string[]> GetElementClassListAsynct(string selector)
        {
            try
            {
                ElementHandle element = await mPage.QuerySelectorAsync(selector);
                JSHandle handler = await element.GetPropertyAsync("classList");
                Dictionary<string, string> value = await handler.JsonValueAsync<Dictionary<string, string>>();
                string[] result = new string[value.Count];

                value.Values.CopyTo(result, 0);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GetElementClassStringAsync(string selector)
        {
            try
            {
                ElementHandle element = await mPage.QuerySelectorAsync(selector);
                JSHandle handler = await element.GetPropertyAsync("classList");
                object value = await handler.JsonValueAsync();
                return value.ToString();
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GetURLAsync(int times = 3, int delay = 3000)
        {
            do
            {
                try
                {
                    return await mPage.EvaluateFunctionAsync<string>(@"
                    () => {
                        return document.URL;
                    }");
                }
                catch
                {
                    await Task.Delay(delay);
                    times--;
                }
            }
            while (times > 0);
            return mPage.Url;
        }
    }
}
